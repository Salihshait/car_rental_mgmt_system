# Supabase database setup for Car Rental

This folder contains a Supabase-ready PostgreSQL migration and seed script for the Car Rental Management System.

## Files
- `001_schema.sql` — core PostgreSQL schema with tables, constraints, and indexes
- `002_seed.sql` — starter seed data for roles, branches, and permissions

## Import into Supabase
1. Open your Supabase project SQL editor.
2. Run `001_schema.sql` first.
3. Run `002_seed.sql` next.

If you use the Supabase CLI, a typical flow is:

```bash
supabase db reset
supabase db push
```

You can also import each file manually through the web SQL editor.
