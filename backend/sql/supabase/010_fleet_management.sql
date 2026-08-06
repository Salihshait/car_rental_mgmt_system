-- Fleet Management: GPS tracking/trips (new), fuel monitoring and
-- maintenance scheduling (wiring up the previously-unused fuel_logs /
-- vehicle_maintenance tables), driver assignment (wiring up the
-- previously-unused drivers table), and vehicle transfers between
-- branches (new).

alter table public.vehicle_maintenance
    add column if not exists completed_at timestamptz,
    add column if not exists notes text,
    add column if not exists created_by uuid references public.users(id) on delete set null,
    add column if not exists created_at timestamptz not null default now();

alter table public.fuel_logs
    add column if not exists odometer_reading int,
    add column if not exists log_type text not null default 'Refuel',
    add column if not exists recorded_by uuid references public.users(id) on delete set null;

alter table public.branches
    add column if not exists latitude numeric(9,6),
    add column if not exists longitude numeric(9,6);

alter table public.vehicles
    add column if not exists current_odometer_reading int;

create table if not exists public.trips (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    driver_id uuid references public.drivers(id) on delete set null,
    started_at timestamptz not null default now(),
    ended_at timestamptz,
    start_latitude numeric(9,6) not null,
    start_longitude numeric(9,6) not null,
    end_latitude numeric(9,6),
    end_longitude numeric(9,6),
    distance_km numeric(10,2) not null default 0,
    status text not null default 'InProgress'
);

create table if not exists public.vehicle_locations (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    trip_id uuid references public.trips(id) on delete set null,
    latitude numeric(9,6) not null,
    longitude numeric(9,6) not null,
    speed_kmh numeric(6,2),
    heading_degrees numeric(6,2),
    recorded_at timestamptz not null default now()
);

create table if not exists public.driver_assignments (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    driver_id uuid not null references public.drivers(id) on delete cascade,
    assigned_at timestamptz not null default now(),
    unassigned_at timestamptz,
    assigned_by uuid not null references public.users(id) on delete restrict,
    notes text
);

create table if not exists public.vehicle_transfers (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    from_branch_id uuid not null references public.branches(id) on delete restrict,
    to_branch_id uuid not null references public.branches(id) on delete restrict,
    requested_by uuid not null references public.users(id) on delete restrict,
    status text not null default 'InTransit',
    requested_at timestamptz not null default now(),
    completed_at timestamptz,
    notes text
);

create index if not exists idx_trips_vehicle_id on public.trips(vehicle_id);
create index if not exists idx_trips_driver_id on public.trips(driver_id);
create index if not exists idx_vehicle_locations_vehicle_id on public.vehicle_locations(vehicle_id);
create index if not exists idx_vehicle_locations_trip_id on public.vehicle_locations(trip_id);
create index if not exists idx_vehicle_locations_recorded_at on public.vehicle_locations(recorded_at);
create index if not exists idx_driver_assignments_vehicle_id on public.driver_assignments(vehicle_id);
create index if not exists idx_driver_assignments_driver_id on public.driver_assignments(driver_id);
create index if not exists idx_vehicle_transfers_vehicle_id on public.vehicle_transfers(vehicle_id);
create index if not exists idx_fuel_logs_vehicle_id on public.fuel_logs(vehicle_id);
create index if not exists idx_vehicle_maintenance_vehicle_id on public.vehicle_maintenance(vehicle_id);
