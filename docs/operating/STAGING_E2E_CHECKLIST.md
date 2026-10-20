# Staging E2E checklist (post-v1 W5)

Apply pending `ERP.Infrastructure/Database/Schemas/20261020_*.sql` scripts on staging before running.

Use a dedicated staging tenant. Preferred client: **frontend-new** (`VITE_API_URL` → staging API).

## Flows

1. **O2C** — Lead/Customer → Quote → SO approve → Dispatch → SI post → payment → (optional) SI unpost with `sales.allowUnpost=true`
2. **P2P** — Vendor → PR/PO → GRN → PI → Bill → payment; Landed cost voucher post
3. **Packaging** — Sale enquiry approve → Pre-cost approve → Quote
4. **SCM depth** — Brand/bin → reservation → cycle count count/post → kit
5. **Leave + MFG** — Leave approve; Production order complete with BOM stock move
6. **Report schedule** — Create schedule with recipients; confirm worker emails when `email.*` SMTP is set

## Sign-off

When all six are green on staging, check off the cross-flows box in [CUSTOMER_DEPTH_PLAN.md](../CUSTOMER_DEPTH_PLAN.md). Until then that tracker item stays open — this checklist is the closeout vehicle, not a substitute for a live run.

SMTP keys (tenant settings): `email.smtpHost`, `email.smtpPort`, `email.smtpUser`, `email.smtpPassword`, `email.fromAddress` (see SettingsCatalog).
