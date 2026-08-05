create table if not exists public.invoices (
    id uuid primary key default gen_random_uuid(),
    booking_id uuid not null unique references public.bookings(id) on delete restrict,
    invoice_number text not null unique,
    issue_date timestamptz not null default now(),
    total_amount numeric(18,2) not null,
    status text not null default 'Unpaid'
);

create table if not exists public.invoice_line_items (
    id uuid primary key default gen_random_uuid(),
    invoice_id uuid not null references public.invoices(id) on delete cascade,
    description text not null,
    amount numeric(18,2) not null
);
