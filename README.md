# voyage-tasks

Your project's `readme` is as important to success as your code. For 
this reason you should put as much care into its creation and maintenance
as you would any other component of the application.

If you are unsure of what should go into the `readme` let this article,
written by an experienced Chingu, be your starting point - 
[Keys to a well written README](https://tinyurl.com/yk3wubft).

And before we go there's "one more thing"! Once you decide what to include
in your `readme` feel free to replace the text we've provided here.

> Own it & Make it your Own!

## Getting started

### Backend (`server/`)

You need the .NET 10 SDK and PostgreSQL running locally. The database itself is created for you by the migration step.

Store your local config once with .NET user secrets. They are saved on your own machine, outside the repo, so a password can never be committed, and they load automatically when you run in Development.

```bash
cd server/src/Ecommerce.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ecommerce_db;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "JwtSettings:AccessTokenSecret" "a long random string, 32 characters or more"
dotnet user-secrets set "JwtSettings:RefreshTokenSecret" "a different long random string"
cd ../../..
```

Everyone sets their own values. Nothing secret is stored in the repo.

Then, from the repo root:

```bash
cd server
dotnet restore
dotnet tool install --global dotnet-ef   # only once
dotnet ef database update --project src/Ecommerce.Infrastructure --startup-project src/Ecommerce.Api
dotnet run --project src/Ecommerce.Api --launch-profile https
```

`dotnet ef database update` creates the tables and the starter data (5 categories, 8 products). The API docs are at `https://localhost:7178/scalar/v1`. The auth and cart cookies are `Secure`, so use the `https` launch profile, not plain http.

### Frontend (`client/`)

```bash
cd client
npm install
npm run dev
```

## Team Documents

You may find these helpful as you work together to organize your project.

- [Team Project Ideas](./docs/team_project_ideas.md)
- [Team Decision Log](./docs/team_decision_log.md)
- API contracts: [Auth](./docs/api-contract-auth.md), [Products](./docs/api-contract-products.md), [Cart](./docs/api-contract-cart.md), [Checkout & Stripe](./docs/api-contract-checkout.md), [Orders](./docs/api-contract-orders.md)
- [Database schema](./docs/database-schema.sql) (reference design, see the note at the top of the file)

Meeting Agenda templates (located in the `/docs` directory in this repo):

- Meeting - Voyage Kickoff --> ./docs/meeting-voyage_kickoff.docx
- Meeting - App Vision & Feature Planning --> ./docs/meeting-vision_and_feature_planning.docx
- Meeting - Sprint Retrospective, Review, and Planning --> ./docs/meeting-sprint_retrospective_review_and_planning.docx
- Meeting - Sprint Open Topic Session --> ./docs/meeting-sprint_open_topic_session.docx

## Our Team

Everyone on your team should add their name along with a link to their GitHub
& optionally their LinkedIn profiles below. Do this in Sprint #1 to validate
your repo access and to practice PR'ing with your team *before* you start
coding!

- Simbongile Mkhotheli: [GitHub](https://github.com/simbongile-mkhotheli) / [LinkedIn](https://linkedin.com/in/mkoteli/)
- Pauline Christelle: [GitHub](https://github.com/pchristelle)] / [LinkedIn](https://www.linkedin.com/in/christelle-nemb-exin-agile-scrum-master/?isSelfProfile=true)]
   ...
- Teammate name #n: [GitHub](https://github.com/ghaccountname) / [LinkedIn](https://linkedin.com/in/liaccountname)
