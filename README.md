# Pars-WoW Master API

Multi-expansion World of Warcraft master data API built with ASP.NET Core 8, Dapper, MySqlConnector, and a clean-architecture DBC engine.

**Supported expansions:** TBC (OregonCore), WOTLK (mangoswotlk), Cataclysm (WoWSourceV10), MoP (EternalCore), Legion (LegionCoreV2).

## Architecture

```
ParsWoW.Api/
├── Application/         # Interfaces, DTOs, Options, Constants, ApiResponse envelope
│   ├── Abstractions/    # Repository, Service, DBC provider interfaces
│   ├── Configuration/   # ParsWowOptions, JwtOptions, AuthOptions, ExpansionOptions
│   ├── Constants/       # ExpansionKind enum, CacheKeys
│   └── DTO/             # Auth, Armory, Account, Shop, DBC public shapes
├── Infrastructure/      # Concrete implementations
│   ├── Auth/            # BlizzCMS-compatible SRP6 emulator hasher, JWT, RefreshTokenStore
│   ├── Cache/           # IMemoryCache wrapper
│   ├── Dbc/             # DBC engine, per-expansion providers, per-expansion schemas
│   │   ├── Engine/      # WdbcReader (raw uint[]), DbcFile, DbcRecord, DbcProviderBase
│   │   ├── Providers/   # TbcDbcProvider, WotlkDbcProvider, CataDbcProvider, MopDbcProvider, LegionDbcProvider
│   │   └── Schemas/     # TBC/, WOTLK/, CATA/, MOP/, LEGION/ — each owns its own column layout
│   ├── Persistence/     # Dapper + MySqlConnector repos; per-expansion connection routing
│   └── Services/        # Auth, Account, Armory, Dbc, Shop, Tooltip
└── Presentation/        # Controllers, Filters, Swagger
    ├── Controllers/     # Auth, Account, Shop, Dbc, Armory, Diagnostics
    ├── Filters/         # ApiExceptionFilter
    └── Swagger/         # Swagger + JWT bearer config
```

## Implementation Status

### DBC Engine & Per-Expansion Schemas ✅
- [x] Generic WdbcReader (raw uint32 rows, no boxing, no heuristics)
- [x] DbcProviderBase — generic loader; expansion-agnostic
- [x] 5 per-expansion IDbcProvider implementations (TBC, WOTLK, CATA, MOP, LEGION)
- [x] Per-expansion Item.dbc + Spell.dbc schema classes (columns isolated per expansion)
- [x] DbcProviderFactory + IDbcService dispatcher

### Authentication ✅
- [x] BlizzCMS-compatible SRP6 emulator-mode password hasher (v, s, legacy sha_pass_hash)
- [x] JWT access-token issuance + validation
- [x] Refresh-token rotation via MySQL-backed store
- [x] POST /api/auth/login, register, refresh, logout, GET /api/auth/me

### Armory ✅
- [x] Character summary, equipment, talents, render, guild endpoints
- [x] Tooltip builder (quality color, stats, enchant placeholder)
- [x] Read-per-expansion routing via ExpansionDatabase enum

### Account Services ✅
- [x] Character rename, race-change, faction-change, appearance-change
- [x] Character unstuck, boost, guild rename
- [x] Ownership validation before any DML

### Shop Purchase ✅
- [x] SKU catalog with deliverable dispatch (item, mount, pet, title, gold, profession)
- [x] Purchase log to MySQL
- [x] Payment validation abstraction (InMemoryPaymentService placeholder)

### Endpoint Summary

| Prefix | Routes | Status |
|--------|--------|--------|
| `GET /api/dbc/{expansion}/item/{entry}` | 5 expansions × each resource | ✅ |
| `GET /api/dbc/{expansion}/spell/{id}` | 5 expansions | ✅ |
| `GET /api/dbc/{expansion}/map/{id}` | 5 expansions | ✅ |
| `GET /api/dbc/{expansion}/area/{id}` | 5 expansions | ✅ |
| `GET /api/dbc/{expansion}/achievement/{id}` | 5 expansions | ✅ |
| `GET /api/armory/{expansion}/character/{realm}/{name}/{summary,equipment,talents,render}` | ✅ |
| `GET /api/armory/{expansion}/guild/{realm}/{name}/summary` | ✅ |
| `POST /api/auth/*` | login, register, refresh, logout | ✅ |
| `GET /api/auth/me` | Requires JWT | ✅ |
| `POST /api/account/{operation}` | rename, race-change, faction-change, etc. | ✅ |
| `POST /api/shop/purchase` | Validate + deliver + log | ✅ |
| `GET /api/diagnostics/health` | ✅ |
| `GET /api/diagnostics/dbc/status` | ✅ |

## Quick Start

```bash
# Prerequisites: .NET 8 SDK, MySQL 8.x

# 1. Create databases
mysql -e "CREATE DATABASE IF NOT EXISTS wotlk_auth"
mysql -e "CREATE DATABASE IF NOT EXISTS wotlk_characters"
mysql -e "CREATE DATABASE IF NOT EXISTS wotlk_world"

# 2. Place DBC files
mkdir -p DBC/{TBC,WOTLK,CATA,MOP,LEGION}
# Copy your .dbc files into the respective expansion folders

# 3. Configure secrets
export PARSWOW_JWT_KEY="your-256-bit-key"
# Or set ParsWow:Jwt:SigningKey in appsettings.json

# 4. Run
cd src/ParsWoW.Api
dotnet run
```

## Next Steps / Work In Progress

1. **BlizzCMS auth DB schema patch** — add `expansion` column to `account` table if missing
2. **Refresh tokens table DDL** — run `CREATE TABLE refresh_tokens (...)`
3. **Per-expansion Armory** — most armory queries default to WOTLK; per-expansion routing needs expansion-resolution via `account.expansion` column
4. **Rankings** — not yet implemented (future scope)
5. **Full per-expansion DBC schemas** — only Item.dbc and Spell.dbc have typed schemas; Map, Area, Achievement need per-expansion projector classes
6. **Improved shop catalog** — SKU definitions live in code (ShopService.cs ≈ line 24); move to DB or config per the user's requirement

## Tests

No test project yet. Run `dotnet test` from `tests/ParsWoW.Api.Tests/` once created.

---

Pars-WoW Development — *"One API to rule them all."*
