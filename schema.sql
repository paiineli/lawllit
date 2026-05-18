-- lawllit.finance — schema PostgreSQL
-- Execute este script para criar o banco do zero.
-- Ordem: Users → Categories → Transactions (FK)

-- ============================================================
-- USERS
-- ============================================================
CREATE TABLE "Users" (
    "Id"                            UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "Name"                          VARCHAR(100)    NOT NULL,
    "Email"                         VARCHAR(255)    NOT NULL,
    "PasswordHash"                  TEXT,
    "GoogleId"                      VARCHAR(255),
    "EmailConfirmed"                BOOLEAN         NOT NULL DEFAULT FALSE,
    "EmailConfirmationToken"        TEXT,
    "EmailConfirmationTokenExpiry"  TIMESTAMPTZ,
    "PasswordResetToken"            TEXT,
    "PasswordResetTokenExpiry"      TIMESTAMPTZ,
    "CreatedAt"                     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "Theme"                         VARCHAR(20)     NOT NULL DEFAULT 'dark',
    "FontSize"                      VARCHAR(20)     NOT NULL DEFAULT 'normal',
    "Language"                      VARCHAR(10)     NOT NULL DEFAULT 'pt-BR',
    "Currency"                      VARCHAR(10)     NOT NULL DEFAULT 'BRL',
    "IsOnboardingCompleted"         BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE UNIQUE INDEX "IX_Users_Email"    ON "Users" ("Email");
CREATE UNIQUE INDEX "IX_Users_GoogleId" ON "Users" ("GoogleId") WHERE "GoogleId" IS NOT NULL;

-- ============================================================
-- CATEGORIES
-- ============================================================
CREATE TABLE "Categories" (
    "Id"        UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "Name"      VARCHAR(100)    NOT NULL,
    "Type"      INTEGER         NOT NULL,   -- 0 = Income, 1 = Expense
    "UserId"    UUID            NOT NULL REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Categories_UserId" ON "Categories" ("UserId");

-- ============================================================
-- TRANSACTIONS
-- ============================================================
CREATE TABLE "Transactions" (
    "Id"            UUID            NOT NULL DEFAULT gen_random_uuid() PRIMARY KEY,
    "Description"   VARCHAR(200),
    "Amount"        NUMERIC(18, 2)  NOT NULL,
    "Type"          INTEGER         NOT NULL,   -- 0 = Income, 1 = Expense
    "Date"          TIMESTAMPTZ     NOT NULL,
    "IsRecurring"   BOOLEAN         NOT NULL DEFAULT FALSE,
    "CreatedAt"     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "UserId"        UUID            NOT NULL REFERENCES "Users"      ("Id") ON DELETE CASCADE,
    "CategoryId"    UUID            NOT NULL REFERENCES "Categories" ("Id")
);

CREATE INDEX "IX_Transactions_UserId"     ON "Transactions" ("UserId");
CREATE INDEX "IX_Transactions_CategoryId" ON "Transactions" ("CategoryId");
CREATE INDEX "IX_Transactions_Date"       ON "Transactions" ("Date" DESC);
