-- CRM Module: customer support tickets, formal complaints, post-booking
-- feedback, reusable outbound message templates, bulk-send campaigns, and a
-- unified outbound message log covering Email/SMS/WhatsApp/Push channels
-- (all simulated for now - no live provider is configured).

create table if not exists public.support_tickets (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete restrict,
    booking_id uuid references public.bookings(id) on delete set null,
    subject text not null,
    category text not null default 'General',
    priority text not null default 'Normal',
    status text not null default 'Open',
    assigned_to_user_id uuid references public.users(id) on delete set null,
    created_at timestamptz not null default now(),
    resolved_at timestamptz
);

create table if not exists public.support_ticket_messages (
    id uuid primary key default gen_random_uuid(),
    ticket_id uuid not null references public.support_tickets(id) on delete cascade,
    sender_user_id uuid not null references public.users(id) on delete cascade,
    is_internal_note boolean not null default false,
    message text not null,
    created_at timestamptz not null default now()
);

create table if not exists public.complaints (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete restrict,
    booking_id uuid references public.bookings(id) on delete set null,
    vehicle_id uuid references public.vehicles(id) on delete set null,
    subject text not null,
    description text not null,
    severity text not null default 'Medium',
    status text not null default 'Open',
    resolution text,
    assigned_to_user_id uuid references public.users(id) on delete set null,
    created_at timestamptz not null default now(),
    resolved_at timestamptz
);

create table if not exists public.feedback (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete restrict,
    booking_id uuid references public.bookings(id) on delete set null,
    rating int not null,
    comment text,
    category text not null default 'General',
    is_published boolean not null default false,
    created_at timestamptz not null default now()
);

create table if not exists public.message_templates (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    channel text not null default 'Email',
    subject text,
    body text not null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz
);

create table if not exists public.campaigns (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    template_id uuid not null references public.message_templates(id) on delete restrict,
    channel text not null default 'Email',
    audience_filter text not null default 'AllCustomers',
    status text not null default 'Draft',
    scheduled_at timestamptz,
    started_at timestamptz,
    completed_at timestamptz,
    target_count int not null default 0,
    sent_count int not null default 0,
    failed_count int not null default 0,
    created_by uuid not null references public.users(id) on delete restrict,
    created_at timestamptz not null default now()
);

create table if not exists public.message_logs (
    id uuid primary key default gen_random_uuid(),
    channel text not null,
    recipient_user_id uuid references public.users(id) on delete set null,
    recipient_address text not null,
    template_id uuid references public.message_templates(id) on delete set null,
    campaign_id uuid references public.campaigns(id) on delete set null,
    subject text,
    body text not null,
    status text not null default 'Simulated',
    provider_message_id text,
    error_message text,
    sent_at timestamptz not null default now()
);

create index if not exists idx_support_tickets_customer_id on public.support_tickets(customer_id);
create index if not exists idx_support_ticket_messages_ticket_id on public.support_ticket_messages(ticket_id);
create index if not exists idx_complaints_customer_id on public.complaints(customer_id);
create index if not exists idx_feedback_customer_id on public.feedback(customer_id);
create index if not exists idx_campaigns_status on public.campaigns(status);
create index if not exists idx_message_logs_channel on public.message_logs(channel);
create index if not exists idx_message_logs_campaign_id on public.message_logs(campaign_id);
