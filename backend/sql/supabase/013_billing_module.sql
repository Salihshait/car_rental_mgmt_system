-- Billing: fixes the partial-payment bug (payments now recalculate a real
-- AmountPaid/Status on the invoice instead of always flipping it to Paid),
-- adds a payment-gateway integration layer (Razorpay/Stripe/Cash/UPI),
-- GST (CGST/SGST/IGST) invoice splitting, standalone refund management, and
-- manual discounts. All additions are on existing tables (payments,
-- invoices, invoice_line_items, refunds, branches, customers) - nothing
-- here needed a new table shape.

alter table public.invoices
    add column if not exists subtotal_amount numeric(18,2) not null default 0,
    add column if not exists discount_amount numeric(18,2) not null default 0,
    add column if not exists cgst_amount numeric(18,2) not null default 0,
    add column if not exists sgst_amount numeric(18,2) not null default 0,
    add column if not exists igst_amount numeric(18,2) not null default 0,
    add column if not exists tax_amount numeric(18,2) not null default 0,
    add column if not exists amount_paid numeric(18,2) not null default 0,
    add column if not exists due_date timestamptz,
    add column if not exists branch_gstin text,
    add column if not exists customer_gstin text,
    add column if not exists place_of_supply text;

-- Backfill so pre-existing invoices still reconcile (subtotal = total when
-- nothing else is known about them).
update public.invoices set subtotal_amount = total_amount where subtotal_amount = 0;

alter table public.invoice_line_items
    add column if not exists item_type text not null default 'Other';

alter table public.payments
    add column if not exists invoice_id uuid references public.invoices(id) on delete set null,
    add column if not exists currency text not null default 'INR',
    add column if not exists gateway_order_id text,
    add column if not exists gateway_signature text,
    add column if not exists gateway_payload jsonb;

alter table public.refunds
    add column if not exists payment_id uuid references public.payments(id) on delete set null,
    add column if not exists reason text,
    add column if not exists refund_method text not null default 'Original',
    add column if not exists gateway text,
    add column if not exists gateway_refund_reference text,
    add column if not exists requested_by uuid references public.users(id) on delete set null,
    add column if not exists processed_by uuid references public.users(id) on delete set null,
    add column if not exists requested_at timestamptz not null default now();

alter table public.branches
    add column if not exists gstin text;

alter table public.customers
    add column if not exists gstin text,
    add column if not exists billing_state text;

create index if not exists idx_payments_invoice_id on public.payments(invoice_id);
create index if not exists idx_refunds_payment_id on public.refunds(payment_id);
