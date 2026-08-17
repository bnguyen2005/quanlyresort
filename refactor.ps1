$directory = "QuanLyResort"
$dbSets = @('Users', 'Customers', 'Rooms', 'Bookings', 'Services', 'Charges', 'Invoices', 'Employees', 'InventoryVouchers', 'AuditLogs', 'Notifications', 'RoomTypes', 'Coupons', 'Reviews', 'Tickets', 'TicketMessages', 'Faqs', 'Settings')

Get-ChildItem -Path $directory -Recurse -Filter "*.cs" | Where-Object {
    $_.FullName -notmatch 'Migrations' -and
    $_.Name -notmatch 'ResortDbContext' -and
    $_.Name -notmatch 'UnitOfWork' -and
    $_.Name -notmatch 'Repository' -and
    $_.Name -notmatch 'Program' -and
    $_.Name -notmatch 'UpdateRoomPrices' -and
    $_.Name -notmatch 'DataSeeder'
} | ForEach-Object {
    $filepath = $_.FullName
    $content = Get-Content -Path $filepath -Raw -Encoding UTF8
    
    if ($content -notmatch 'ResortDbContext\s*_context' -and $content -notmatch 'ResortDbContext\?\s*_context') {
        return
    }

    $original = $content
    $hasUow = $content -match 'IUnitOfWork _unitOfWork'

    if (-not $hasUow) {
        $content = $content -replace 'private readonly ResortDbContext _context;', 'private readonly IUnitOfWork _unitOfWork;'
        $content = $content -replace 'private readonly ResortDbContext\? _context;', 'private readonly IUnitOfWork _unitOfWork;'
        $content = $content -replace 'ResortDbContext\s+context', 'IUnitOfWork unitOfWork'
        $content = $content -replace 'ResortDbContext\?\s+context', 'IUnitOfWork unitOfWork'
        $content = $content -replace '_context\s*=\s*context;', '_unitOfWork = unitOfWork;'
    } else {
        $content = $content -replace '\s*private readonly ResortDbContext _context;\s*', "
"
        $content = $content -replace '\s*private readonly ResortDbContext\? _context;\s*', "
"
        $content = $content -replace ',\s*ResortDbContext\s+context', ''
        $content = $content -replace 'ResortDbContext\s+context,\s*', ''
        $content = $content -replace ',\s*ResortDbContext\?\s+context', ''
        $content = $content -replace 'ResortDbContext\?\s+context,\s*', ''
        $content = $content -replace '\s*_context\s*=\s*context;\s*', "
"
    }

    foreach ($dbSet in $dbSets) {
        $content = $content -replace "_context\.\\.Add\(", "_unitOfWork..AddAsync("
        $content = $content -replace "_context\.\\.Update\(", "_unitOfWork..Update("
        $content = $content -replace "_context\.\\.Remove\(", "_unitOfWork..Remove("
        $content = $content -replace "_context\.\\.AddRange\(", "_unitOfWork..AddRangeAsync("
        $content = $content -replace "_context\.\\.RemoveRange\(", "_unitOfWork..RemoveRange("
        
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, "_context\.(?!\w)", "_unitOfWork..Query()")
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, "_context\?\.(?!\w)", "_unitOfWork..Query()")
    }

    $content = $content -replace '_context\.SaveChangesAsync\(\)', '_unitOfWork.SaveChangesAsync()'
    $content = $content -replace '_context\?\.SaveChangesAsync\(\)', '_unitOfWork.SaveChangesAsync()'
    $content = $content -replace '_context\.Database\.BeginTransactionAsync\(\)', '_unitOfWork.BeginTransactionAsync()'

    if ($content -cne $original) {
        if ($content -notmatch 'using QuanLyResort.Repositories;' -and $content -match 'IUnitOfWork') {
            $content = "using QuanLyResort.Repositories;
" + $content
        }
        Set-Content -Path $filepath -Value $content -Encoding UTF8
        Write-Host "Refactored: $filepath"
    }
}
Write-Host "Refactoring script finished."
