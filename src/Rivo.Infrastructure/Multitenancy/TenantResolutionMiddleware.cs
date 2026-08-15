namespace Rivo.Infrastructure.Multitenancy;

// Разрешение tenant'а сделано декларативно через JWT-claim "tenant_id" (см. TenantService)
// и проверяется в API/Middlewares/TenantMiddleware — отдельный middleware здесь не нужен,
// чтобы не дублировать логику. Оставлено как заготовка на случай резолюции tenant'а не из токена
// (например, по поддомену/заголовку) — не понадобилось в Phase A.
