-- AI Module: 6 data-driven analytics features compute directly from existing
-- tables (no schema needed beyond FraudAlert/MaintenancePrediction below), and
-- 5 perception/NLP features get simulated-provider result tables so their
-- outputs are recorded and reviewable.

create table if not exists public.fraud_alerts (
    id uuid primary key default gen_random_uuid(),
    booking_id uuid references public.bookings(id) on delete set null,
    payment_id uuid references public.payments(id) on delete set null,
    customer_id uuid not null references public.users(id) on delete cascade,
    risk_score int not null default 0,
    reasons text not null default '',
    status text not null default 'Open',
    created_at timestamptz not null default now(),
    reviewed_by uuid references public.users(id) on delete set null,
    reviewed_at timestamptz
);

create table if not exists public.maintenance_predictions (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    predicted_issue text not null,
    predicted_due_date timestamptz not null,
    confidence_score numeric(5,2) not null default 0,
    basis_summary text not null default '',
    status text not null default 'Open',
    created_at timestamptz not null default now()
);

create table if not exists public.damage_detection_results (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid references public.vehicles(id) on delete set null,
    rental_id uuid references public.rentals(id) on delete set null,
    image_reference text not null,
    detected_damages_json text not null default '[]',
    severity_score numeric(5,2) not null default 0,
    created_at timestamptz not null default now()
);

create table if not exists public.document_ocr_results (
    id uuid primary key default gen_random_uuid(),
    document_type text not null,
    extracted_fields_json text not null default '{}',
    confidence_score numeric(5,2) not null default 0,
    created_by_user_id uuid not null references public.users(id) on delete cascade,
    created_at timestamptz not null default now()
);

create table if not exists public.chat_sessions (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid references public.users(id) on delete set null,
    channel text not null default 'Web',
    started_at timestamptz not null default now(),
    last_message_at timestamptz not null default now()
);

create table if not exists public.chat_messages (
    id uuid primary key default gen_random_uuid(),
    session_id uuid not null references public.chat_sessions(id) on delete cascade,
    sender text not null default 'Customer',
    message text not null,
    created_at timestamptz not null default now()
);

create table if not exists public.voice_booking_requests (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete cascade,
    transcribed_text text not null default '',
    parsed_intent_json text not null default '{}',
    status text not null default 'Transcribed',
    created_at timestamptz not null default now()
);

create index if not exists idx_fraud_alerts_status on public.fraud_alerts(status);
create index if not exists idx_maintenance_predictions_vehicle_id on public.maintenance_predictions(vehicle_id);
create index if not exists idx_chat_messages_session_id on public.chat_messages(session_id);
create index if not exists idx_voice_booking_requests_customer_id on public.voice_booking_requests(customer_id);
