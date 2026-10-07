# Mocidade 015

Sistema web de reserva de assentos em ônibus para a viagem da mocidade. Cada participante cria uma conta, cadastra seus acompanhantes, escolhe o ônibus pelo terminal de saída e marca o assento em um mapa. Quando o terminal lota, o participante entra na lista de espera e o administrador distribui as vagas que surgirem.

## Funcionalidades

**Participante**
- Cadastro e login com senha
- Cadastro de acompanhantes
- Escolha do ônibus por terminal de saída e do assento no mapa
- Lista de espera por terminal quando não há vagas
- Orientação de pagamento via WhatsApp após a reserva
- Edição do perfil

**Administrador**
- Painel com a ocupação de cada ônibus e terminal
- Cancelamento de reservas, liberando o assento
- Criação de um novo ônibus para um terminal
- Atribuição de vagas a quem está na lista de espera

## Tecnologias

- ASP.NET Core 10 com Razor Pages
- Entity Framework Core 10 com Npgsql
- PostgreSQL (Supabase)
- Autenticação por cookie e senhas com BCrypt
- Bootstrap 5 e Bootstrap Icons
- GitHub Actions para o deploy

## Estrutura

```
Mocidade015/
  Data/         AppDbContext e mapeamento das tabelas
  Models/       Entidades, view models e opções tipadas
  Pages/        Páginas públicas (Login, Cadastro, Perfil)
    App/        Área do participante (exige login)
    Admin/      Painel administrativo (exige papel Admin)
  Services/     Regras de reserva, validadores e serviço de rate limit
  wwwroot/      CSS, JavaScript e imagens
.github/workflows/deploy.yml   Publicação automática
Dockerfile                     Imagem para execução em container
```

## Como executar

Pré-requisitos: [.NET SDK 10](https://dotnet.microsoft.com/download) e um banco PostgreSQL com o schema já criado.

1. Informe a connection string. Ela nunca fica no `appsettings.json`:

   ```bash
   dotnet user-secrets set "ConnectionStrings:SupabaseConnection" "Host=...;Database=...;Username=...;Password=..." --project Mocidade015
   ```

2. Execute:

   ```bash
   dotnet run --project Mocidade015
   ```

3. Acesse http://localhost:5008.

O projeto não versiona migrações do EF Core: as tabelas (`Usuarios`, `Onibus`, `Assentos`, `Acompanhantes`, `Reservas`, `ListaEspera`) são mantidas diretamente no banco e mapeadas em `Data/AppDbContext.cs`.

## Configuração

| Chave | Onde | Descrição |
|---|---|---|
| `ConnectionStrings__SupabaseConnection` | Variável de ambiente ou User Secrets | Conexão com o PostgreSQL. Obrigatória |
| `Viagem:Data` | `appsettings.json` | Data da viagem |
| `Viagem:LotacaoPadrao` | `appsettings.json` | Assentos por ônibus |
| `Viagem:HorariosPorTerminal` | `appsettings.json` | Terminais de saída e seus horários |
| `Contato:WhatsappPagamento` | `appsettings.json` | Número que recebe os comprovantes |

Toda conta nasce com o papel `Cliente`. Para criar um administrador, altere a coluna `role` do usuário para `Admin` no banco.

## Segurança e consistência

- Reservas, cancelamentos e atribuições de vaga rodam em transação `Serializable`, o que impede dois participantes de ficarem com o mesmo assento.
- O mesmo passageiro não pode ter duas reservas no mesmo ônibus.
- Proteção antiforgery e cookies `HttpOnly` e `Secure`.
- `RateLimitService` está registrado, mas ainda não é aplicado às páginas de login e cadastro.
- As pastas `/App` e `/Admin` são protegidas por convenção de autorização.

## Deploy

**VPS (automático).** Cada push na `main` dispara `.github/workflows/deploy.yml`, que publica o projeto, envia os arquivos por rsync e reinicia o serviço `systemd`. Segredos necessários no repositório: `SSH_KEY`, `HOST` e `USERNAME`.

**Container.** O `Dockerfile` da raiz gera uma imagem que escuta na porta 8080:

```bash
docker build -t mocidade015 .
docker run -p 8080:8080 -e ConnectionStrings__SupabaseConnection="..." mocidade015
```

## Documentação complementar

A pasta `Mocidade015/` traz relatórios de análise e guias de implementação. Comece por [`Mocidade015/00_COMECE_AQUI.md`](Mocidade015/00_COMECE_AQUI.md).
