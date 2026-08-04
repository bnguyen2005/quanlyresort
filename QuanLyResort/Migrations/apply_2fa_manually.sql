-- Manual SQL script to add 2FA fields to Users table
-- Run this if migrations are not working

-- Check if columns already exist before adding
-- For SQLite
ALTER TABLE Users ADD COLUMN TwoFactorSecret TEXT NULL;
ALTER TABLE Users ADD COLUMN TwoFactorEnabled INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Users ADD COLUMN TwoFactorEnabledAt TEXT NULL;
ALTER TABLE Users ADD COLUMN TwoFactorRecoveryCodes TEXT NULL;

-- Note: SQLite doesn't support ALTER TABLE ADD COLUMN IF NOT EXISTS
-- If columns already exist, you'll get an error - that's okay, just ignore it

