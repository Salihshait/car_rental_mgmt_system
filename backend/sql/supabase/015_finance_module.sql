-- Finance Module: bank accounts + manual bank transactions, and manual
-- journal entries for income/expense not already captured elsewhere.
-- Income, Expenses, Cashbook, Ledger, Profit & Loss, Balance Sheet, and GST
-- Reports are all computed by aggregating these tables together with the
-- existing payments/invoices/maintenance_expenses/driver_salary_payments/
-- refunds tables - no further schema is needed for those.

create table if not exists public.bank_accounts (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    account_number text not null,
    bank_name text not null,
    branch_id uuid references public.branches(id) on delete set null,
    opening_balance numeric(18,2) not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null default now()
);

create table if not exists public.bank_transactions (
    id uuid primary key default gen_random_uuid(),
    bank_account_id uuid not null references public.bank_accounts(id) on delete cascade,
    transaction_date timestamptz not null default now(),
    type text not null default 'Credit',
    amount numeric(18,2) not null,
    category text not null default 'General',
    description text,
    created_by uuid not null references public.users(id) on delete restrict,
    created_at timestamptz not null default now()
);

create table if not exists public.journal_entries (
    id uuid primary key default gen_random_uuid(),
    entry_date timestamptz not null default now(),
    entry_type text not null default 'Income',
    category text not null default 'General',
    description text not null,
    amount numeric(18,2) not null,
    bank_account_id uuid references public.bank_accounts(id) on delete set null,
    created_by uuid not null references public.users(id) on delete restrict,
    created_at timestamptz not null default now()
);

create index if not exists idx_bank_transactions_bank_account_id on public.bank_transactions(bank_account_id);
create index if not exists idx_bank_transactions_transaction_date on public.bank_transactions(transaction_date);
create index if not exists idx_journal_entries_entry_date on public.journal_entries(entry_date);
