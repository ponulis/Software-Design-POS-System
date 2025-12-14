# Deployment Configuration Guide

## Environment Variables

The application uses environment variables for configuration. Set these in your production environment:

### Required Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to `Production` for production deployments
- `ConnectionStrings__DefaultConnection` - SQL Server connection string
- `JwtSettings__SecretKey` - JWT signing key (must be at least 32 characters)
- `StripeSettings__SecretKey` - Stripe secret key for payment processing
- `StripeSettings__PublishableKey` - Stripe publishable key

### Optional Environment Variables

- `JwtSettings__Issuer` - JWT issuer (default: "POSSystem")
- `JwtSettings__Audience` - JWT audience (default: "POSSystemUsers")
- `JwtSettings__ExpirationMinutes` - JWT expiration in minutes (default: 60)
- `CorsSettings__AllowedOrigins__0` - First allowed CORS origin
- `CorsSettings__AllowedOrigins__1` - Second allowed CORS origin (add more as needed)
- `SwaggerSettings__EnableSwagger` - Enable Swagger UI (default: false in production)

### Example Configuration

```bash
# Production environment
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Server=your-server;Database=POSSystemDb;User Id=user;Password=pass;"
JwtSettings__SecretKey="your-super-secret-key-at-least-32-characters-long"
StripeSettings__SecretKey="sk_live_your_stripe_secret_key"
StripeSettings__PublishableKey="pk_live_your_stripe_publishable_key"
CorsSettings__AllowedOrigins__0="https://yourdomain.com"
SwaggerSettings__EnableSwagger="false"
```

## CORS Configuration

CORS is configured via `appsettings.json` or environment variables. For production:

1. Set `CorsSettings:AllowedOrigins` in `appsettings.Production.json` or via environment variables
2. Only include trusted frontend origins
3. Do not use wildcards in production

## Swagger Documentation

- Swagger is enabled by default in Development environment
- In Production, set `SwaggerSettings:EnableSwagger` to `false` for security
- Access Swagger UI at `/swagger` when enabled

## Database Migrations

To apply database migrations in production:

```bash
dotnet ef database update --project backend --context ApplicationDbContext
```

## Security Considerations

1. **JWT Secret Key**: Use a strong, randomly generated secret key (at least 32 characters)
2. **Connection Strings**: Never commit connection strings with credentials to version control
3. **Stripe Keys**: Use live keys only in production, test keys in development
4. **CORS**: Restrict allowed origins to known frontend domains
5. **Swagger**: Disable Swagger UI in production to prevent API exposure
