# H2-Projekt {Indsæt gruppenavn}

Projektet han findes her - [H2 Projekt forløb på Notion](https://mercantec.notion.site/h2f)

Det er delt op i 4 mapper (3 hovedprojekter og Aspire)

## [Blazor](/Blazor/)

Vi anbefaler at I bruger Blazor WebAssembly, da det er det vi underviser i. Den er koblet op på vores API gemmen APIService klassen i Blazor.

## [Domain Models](/DomainModels/)

Her er alle jeres klasser, som I skal bruge til jeres Blazor og API.
Domain Models / Class Libary versionen er nu opdateret til .NET 9.0

## [API](/API/)

Her er jeres API, den bruger vi til at forbinde sikkert til vores database og for at fodre data til vores Blazor Projekt.
ASP.NET Core Web API versionen er nu opdateret til .NET 9.0

## [Aspire](/H2-Projekt.AppHost/)

Aspire er vores hosting platform, den er koblet op til vores API og Blazor. Det er ikke obligatorisk at bruge Aspire, men det anbefales. Vi bruger Aspire med .NET 9.0

### Hosting

Vi udforsker forskellige hosting muligheder på H2 - men vil helst vores lokale datacenter. På H2 bruger vi Windows Server 2022 som platform - det introducerede vi senere i forløbet.


Oleh Reflection 
Test af registrering og login (Swagger og Postman)
Registrering og login blev testet i Swagger UI og Postman. Begge endpoints giver de rigtige svar, og ved succesfuld login bliver der udstedt et JWT-token.
Test af JWT-beskyttet adgang til /me
Endpointet /me blev testet med og uden et gyldigt JWT-token. Adgang gives kun med et gyldigt token, så beskyttelsen virker korrekt.
  Kort beskrivelse af API i README
API’et giver endpoints til registrering af bruger, login (med JWT) og styring af brugerprofil. Der er også funktioner til hotel, værelser og booking, med sikker adgang til brugerens data.
  Release 1 – Opsummering
Opgaver vi har lavet:
* Database & EF Core: Lavet User-model, sat AppDBContext op, og kørt første migration.
* Brugerhåndtering: Registrering og login med BCrypt password hashing.
* Database relationer & DTO: Tilføjet UserInfo, Hotel, Room, Booking med relationer og DTO’er.
* JWT & sikkerhed: Sat JWT op til login/validering og beskyttet /me.
  Refleksion
Tekniske udfordringer og løsninger: Jeg havde problemer med relationer mellem modeller og nullable felter. Jeg løste det ved at ændre modellerne og bruge navigation properties i EF Core.
Læring om database design og relationer: Jeg lærte at lave normaliserede tabeller og sætte relationer som én-til-én og én-til-mange i EF Core.  Men jeg føler mig stadig usikker og skal øve mere.
Erfaring med JWT og sikkerhed: Jeg har prøvet at beskytte endpoints med JWT og sætte authentication middleware op. Men det var meget svært at forstå, så jeg føler at jeg kun har forstået det i teorien, ikke helt i praksis.
  Hvis vi startede forfra:
Jeg ville planlægge modeller og relationer bedre fra starten, så jeg ikke behøver at lave om senere.
  Mest og mindst sikre områder:
* Jeg er mest sikker på EF Core migrationer og basis login.
* Jeg er mindst sikker på avanceret sikkerhed og DTO mapping.  Ting jeg skal lære mere om:
* Avanceret JWT sikkerhed
* Refresh tokens
* Bedste praksis for DTO og validering
* 



