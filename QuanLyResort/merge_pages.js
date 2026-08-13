const fs = require('fs');
const path = require('path');

const basePath = path.join('d:', 'quanlyresort-main', 'quanlyresort-main', 'QuanLyResort', 'wwwroot', 'customer');

// 1. Read files
let indexHtml = fs.readFileSync(path.join(basePath, 'index.html'), 'utf-8');
const roomsHtml = fs.readFileSync(path.join(basePath, 'rooms.html'), 'utf-8');
const restaurantHtml = fs.readFileSync(path.join(basePath, 'restaurant.html'), 'utf-8');
const reviewsHtml = fs.readFileSync(path.join(basePath, 'reviews.html'), 'utf-8');

// 2. Extract sections
function extractTag(content, tag, idOrClassRegex) {
    const start = content.indexOf(`<${tag} ${idOrClassRegex}`);
    if (start === -1) return '';
    // simple matching for closing tag (assumes no nesting of same tag type at root level of extraction, but it's safe enough for main/scripts)
    // Actually, better to use regex or string parsing
}

// Since HTML parsing with regex is hard, let's extract by knowing the file structure:
// In rooms.html, the main content is inside <main class="ts-snap-container" id="snapContainer"> ... </main>
let roomsContentMatch = roomsHtml.match(/<main class="ts-snap-container" id="snapContainer">([\s\S]*?)<\/main>/i);
let roomsContent = roomsContentMatch ? roomsContentMatch[1] : '';

// In restaurant.html
let restaurantContentMatch = restaurantHtml.match(/<main class="ts-snap-container" id="snapContainer">([\s\S]*?)<\/main>/i);
let restaurantContent = restaurantContentMatch ? restaurantContentMatch[1] : '';

// In reviews.html, it doesn't have snapContainer yet. Let's extract the main body and wrap it.
// reviews.html has <section class="ftco-section ftco-no-pt ftco-no-pb"> ... </section>
// Let's just grab the whole body minus scripts and header
let reviewsBodyMatch = reviewsHtml.match(/<body>([\s\S]*?)<!-- Scripts -->/i);
let reviewsContent = reviewsBodyMatch ? reviewsBodyMatch[1] : '';
// Remove header-placeholder from reviews
reviewsContent = reviewsContent.replace(/<div id="header-placeholder"><\/div>/i, '');
// Wrap reviews in a slide
reviewsContent = `
<!-- ═══ Slide: REVIEWS ═══ -->
<section class="ts-slide" id="section-reviews" style="overflow-y: auto;">
  <div style="padding-top: 80px; width: 100%;">
    ${reviewsContent}
  </div>
</section>
`;

// 3. Extract Scripts
let roomsScriptsMatch = roomsHtml.match(/<script>([\s\S]*?)<\/script>/i);
let roomsScript = roomsScriptsMatch ? roomsScriptsMatch[1] : '';

let restaurantScriptsMatch = restaurantHtml.match(/<script>([\s\S]*?)<\/script>/ig); // Might be multiple
let restaurantScript = restaurantScriptsMatch ? restaurantScriptsMatch.join('\n') : '';
// For restaurant.html, there's a big <script> tag for menu
let restBigScript = restaurantHtml.substring(restaurantHtml.indexOf('// Global variables for menu management'));
if (restBigScript) {
   let endIndex = restBigScript.indexOf('</script>');
   if (endIndex > -1) restBigScript = restBigScript.substring(0, endIndex);
}

let reviewsScriptMatch = reviewsHtml.match(/<script>([\s\S]*?)<\/script>/ig);
let reviewsScript = reviewsScriptMatch ? reviewsScriptMatch.join('\n') : '';

// 4. Merge into index.html
// Replace </main> in index.html with the new contents + </main>
const endMainIndex = indexHtml.indexOf('</main>');
if (endMainIndex > -1) {
    let mergedMain = `
    <!-- ================= ROOMS ================= -->
    ${roomsContent.replace('class="ts-slide ts-intro-slide"', 'class="ts-slide ts-intro-slide" id="section-rooms"')}
    
    <!-- ================= RESTAURANT ================= -->
    <div id="section-restaurant" style="display:contents;">
      ${restaurantContent}
    </div>
    
    <!-- ================= REVIEWS ================= -->
    ${reviewsContent}
    
    </main>
    `;
    
    indexHtml = indexHtml.substring(0, endMainIndex) + mergedMain + indexHtml.substring(endMainIndex + 7);
}

// 5. Append Scripts to index.html
// Replace </script>\n  <script src="js/page-transition.js"></script> with all scripts
const scriptInsertIndex = indexHtml.lastIndexOf('<script src="js/page-transition.js"></script>');
if (scriptInsertIndex > -1) {
    let mergedScripts = `
    <script>
      /* --- ROOMS SCRIPT --- */
      ${roomsScript}
      
      /* --- RESTAURANT SCRIPT --- */
      ${restBigScript}
      
      /* --- REVIEWS SCRIPT --- */
      ${reviewsScript.replace(/<script>/g, '').replace(/<\/script>/g, '')}
    </script>
    <script src="js/page-transition.js"></script>
    `;
    indexHtml = indexHtml.substring(0, scriptInsertIndex) + mergedScripts + indexHtml.substring(scriptInsertIndex + 45);
}

// Make sure ts-rooms-container uses display: contents
indexHtml = indexHtml.replace('<div id="ts-rooms-container"></div>', '<div id="ts-rooms-container" style="display: contents;"></div>');

// Write back to index.html
fs.writeFileSync(path.join(basePath, 'index.html'), indexHtml, 'utf-8');

console.log('Merged successfully!');
