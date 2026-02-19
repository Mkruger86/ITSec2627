# FlatFileRepo – Flat-file JSON brugerdatabase
***Skrevet i C#, Testmiljøet under VS's indbyggede MSTest***

Brugerobjekt:
`{person_id, first_name, last_name, address, street_number, password, enabled}`  
(I koden: `Person_Id, First_Name, Last_Name, Address, Street_Number, Password, Enabled`)

---

## Hvorfor det er smart at bruge en flat_file_db
En flat-file DB (JSON i fil) kan være hensigtsmæssig når:
- Datasættet er lille og simpelt.
- Man vil undgå afhængighed af en database-server (ingen opsætning af SQL/PostgreSQL osv.).
- Data skal være let at inspicere, flytte og versionere (filen kan ligge i et kendt katalog).
- Man vil demonstrere persistens + CRUD hurtigt i et undervisningsprojekt.

Begrænsninger:
- Ingen samtidighedskontrol (to processer kan skrive samtidig og overskrive hinanden).
- Ingen transaktioner/rollback.
- Potentielt datatab ved nedbrud midt i skrivning.
- Password gemmes i klartekst (sikkerhedsrisiko).

---

## System under test
### Data-model
`PersonModel` med felter:
- `Person_Id` (int)
- `First_Name` (string)
- `Last_Name` (string)
- `Address` (string)
- `Street_Number` (int)
- `Password` (string)
- `Enabled` (bool)

### Validering (funktioner i PersonModel)
- `ValidateName()`
- `ValidateAddress()`
- `ValidatePassword()`
- `ValidateEnabled()`
- `ValidatePerson()` (samler de andre)

### Repository (flat-file persistens)
`PersonRepository(filePath)`:
- Loader fra JSON-fil ved opstart
- `GetAll()`, `GetById(id)`
- `Add(person)` (validerer + skriver fil)
- `Update(id, updated)` (validerer + skriver fil)
- `Remove(id)` (skriver fil)

### Simpel GUI til CRUD operationer

---

## Test Pyramiden (hvilke testtyper vi bruger)
Vi tester i 3 lag:

1. **Unit tests (mange)**
   - PersonModel valideringsregler (navn, adresse, password).
   - Repository-metoder hvor det giver mening at teste logik isoleret.

2. **Integration tests (nogle)**
   - File-backed repository mod en midlertidig JSON-fil:
     - Add/Update/Remove ændrer filen korrekt
     - Repository loader korrekt ved start (persistens)

3. **E2E/Console tests (få eller ingen)**
   - Konsolmenu kan testes indirekte via integration tests af repository.
   - (Hvis tid: en enkelt test der simulerer input, men ikke nødvendig for at bevise DB-funktionerne.)

---

## CRUD(L) – krav og testmål
| Operation | Funktion | Forventning |
|---|---|---|
| Create | `Add(person)` | Ny bruger får nyt `Person_Id`, gemmes i fil |
| Read | `GetById(id)` | Finder korrekt bruger eller `null` |
| Update | `Update(id, updated)` | Opdaterer felter og gemmer i fil |
| Delete | `Remove(id)` | Fjerner bruger og gemmer i fil |
| List | `GetAll()` | Returnerer alle brugere |

---

## Ækvivalensklasser (Equivalence Classes)

### 1) Name (First_Name, Last_Name)
Regler i kode:
- Må ikke være null/empty/whitespace
- Længde skal være 1..25 (bemærk: implementeringen har en redundant check, men intentionen er 1..25)

| Inputklasse | Beskrivelse | Gyldig? | Relevant test-id |
|---|---|---:|---|
| N1 | `null` | Nej | TC-N-01 |
| N2 | `""` (tom) | Nej | TC-N-02 |
| N3 | `"   "` (whitespace) | Nej | TC-N-03 |
| N4 | længde 1..25 | Ja | TC-N-04 |
| N5 | længde >25 | Nej | TC-N-05 |

### 2) Address + Street_Number
Regler i kode:
- `Address` må ikke være null/empty/whitespace
- `Street_Number` skal være > 0

| Inputklasse | Beskrivelse | Gyldig? | Relevant test-id |
|---|---|---:|---|
| A1 | `Address = null/""/"   "` | Nej | TC-A-01 |
| A2 | `Address` normal tekst | Ja | TC-A-02 |
| S1 | `Street_Number <= 0` | Nej | TC-S-01 |
| S2 | `Street_Number > 0` | Ja | TC-S-02 |

### 3) Password
Regler i kode:
- Må ikke være null/empty/whitespace
- Regex: mindst 6 tegn, mindst ét bogstav og ét tal

| Inputklasse | Beskrivelse | Gyldig? | Relevant test-id |
|---|---|---:|---|
| P1 | `null/""/"   "` | Nej | TC-P-01 |
| P2 | < 6 tegn | Nej | TC-P-02 |
| P3 | >= 6 tegn, kun bogstaver | Nej | TC-P-03 |
| P4 | >= 6 tegn, kun tal | Nej | TC-P-04 |
| P5 | >= 6 tegn, bogstav + tal | Ja | TC-P-05 |

### 4) Enabled
I C# er `bool` altid `true` eller `false`, så der findes reelt ingen ugyldig klasse så længe typen er `bool`.
(Valideringsmetoden `ValidateEnabled()` kan aldrig fejle i nuværende form.)

| Inputklasse | Beskrivelse | Gyldig? | Note |
|---|---|---:|---|
| E1 | `true` | Ja | altid gyldig |
| E2 | `false` | Ja | altid gyldig |

---

## Grænseværditest (Boundary Value Analysis)
Fokus på værdier lige ved grænserne:

### Name længde (1..25)
| Felt | Værdi | Forventning | Test-id |
|---|---|---|---|
| First_Name | længde 0 | fejl | TC-B-Name-00 |
| First_Name | længde 1 | ok | TC-B-Name-01 |
| First_Name | længde 25 | ok | TC-B-Name-25 |
| First_Name | længde 26 | fejl | TC-B-Name-26 |

### Street_Number (>0)
| Felt | Værdi | Forventning | Test-id |
|---|---|---|---|
| Street_Number | 0 | fejl | TC-B-Street-00 |
| Street_Number | 1 | ok | TC-B-Street-01 |
| Street_Number | -1 | fejl | TC-B-Street--1 |

### Password (min 6, kræver bogstav+tal)
| Felt | Værdi | Forventning | Test-id |
|---|---|---|---|
| Password | længde 5 (med bogstav+tal) | fejl | TC-B-Pw-05 |
| Password | længde 6 (med bogstav+tal) | ok | TC-B-Pw-06 |

---

## Decision Table Test (kombinationer af validering)
Vi tester kombinationer af regler i `ValidatePerson()` uden at eksplodere i antal cases.

Betingelser:
- C1: Name valid? (First+Last)
- C2: Address valid?
- C3: Street_Number valid?
- C4: Password valid?
Forventning:
- Accept (ingen exception) eller Reject (ArgumentException)

| Regel | C1 | C2 | C3 | C4 | Forventning | Test-id |
|---|---:|---:|---:|---:|---|---|
| R1 | Y | Y | Y | Y | Accept | TC-DT-01 |
| R2 | N | Y | Y | Y | Reject | TC-DT-02 |
| R3 | Y | N | Y | Y | Reject | TC-DT-03 |
| R4 | Y | Y | N | Y | Reject | TC-DT-04 |
| R5 | Y | Y | Y | N | Reject | TC-DT-05 |
| R6 | N | N | Y | Y | Reject | TC-DT-06 |
| R7 | Y | N | N | Y | Reject | TC-DT-07 |
| R8 | Y | Y | N | N | Reject | TC-DT-08 |

---

## Cycle Process Test (persistens / livscyklus)
Målet er at bevise at `people.json` er primær storage og at data overlever programstop.

Cycle definition:
1. Start repository (loader fra fil)
2. Create (Add)
3. Stop (simuler ved at oprette nyt repository)
4. Read (GetById/GetAll) -> data findes stadig
5. Update
6. Stop -> ny repository
7. Read -> ændringer findes stadig
8. Delete
9. Stop -> ny repository
10. Read -> data er væk

| Trin | Handling | Forventning | Test-id |
|---|---|---|---|
| 1-4 | Add -> ny repo -> read | Data persistet | TC-CYCLE-01 |
| 5-7 | Update -> ny repo -> read | Data opdateret persistet | TC-CYCLE-02 |
| 8-10 | Delete -> ny repo -> read | Data fjernet persistet | TC-CYCLE-03 |

---

## Test Cases (oversigt med Given/When/Then + risiko)
Nedenfor er de testcases vi vil implementere. Hver test skal indeholde kommentarer:
- `// given`
- `// when`
- `// then`

Og en kort risikokommentar:
- `// risk: ...`

### Unit tests – PersonModel validering
| Test-id | Testnavn (forslag) | Given | When | Then | Risiko hvis fejler |
|---|---|---|---|---|---|
| TC-N-01 | `ValidateName_FirstNameNull_ThrowsArgumentException` | First_Name = null | ValidateName | exception | Ugyldige brugere kan oprettes (manglende navn) |
| TC-N-04 | `ValidateName_LengthWithinRange_DoesNotThrow` | længde 1..25 | ValidateName | ingen exception | Brugere kan fejlagtigt afvises |
| TC-A-01 | `ValidateAddress_AddressWhitespace_ThrowsArgumentException` | Address="   " | ValidateAddress | exception | Ugyldig adresse kan gemmes |
| TC-S-01 | `ValidateAddress_StreetNumberZero_ThrowsArgumentException` | Street_Number=0 | ValidateAddress | exception | Ugyldig street_number kan gemmes |
| TC-P-05 | `ValidatePassword_ValidPassword_DoesNotThrow` | "abc123" | ValidatePassword | ingen exception | Gyldige passwords afvises |
| TC-P-03 | `ValidatePassword_LettersOnly_ThrowsArgumentException` | "abcdef" | ValidatePassword | exception | Svage passwords accepteres |

### Integration tests – PersonRepository + fil
(Disse bruger en midlertidig fil pr. test.)

| Test-id | Testnavn (forslag) | Given | When | Then | Risiko hvis fejler |
|---|---|---|---|---|---|
| TC-CRUD-01 | `Add_ValidPerson_PersistsToJsonFile` | tom fil | Add(person) | fil indeholder person med id | DB gemmer ikke data |
| TC-CRUD-02 | `GetById_ExistingId_ReturnsPerson` | fil med person | GetById(id) | korrekt person | Read virker ikke |
| TC-CRUD-03 | `Update_ExistingId_PersistsChanges` | fil med person | Update(id, changed) | fil viser ændringer | Opdateringer går tabt |
| TC-CRUD-04 | `Remove_ExistingId_RemovesFromFile` | fil med person | Remove(id) | fil har ikke person | Sletning virker ikke |
| TC-CYCLE-01 | `Cycle_AddRestart_ReadStillExists` | tom fil | Add -> new repo -> GetAll | stadig der | Ingen persistens over restart |

---

## Screenshots (indsættes når tests er implementeret)
- Screenshot af test-run (unit + integration)
- Screenshot af tests med tydelige navne
- Screenshot der viser `Given/When/Then` kommentarer og `risk:` kommentar

---

## Noter / kendte forbedringer
- `ValidateEnabled()` kan aldrig fejle med `bool` i C#. Hvis der ønskes validering af “ukendt”, skal `Enabled` være `bool?`.
- Password gemmes i klartekst i JSON. I et rigtigt system bør det hashes og saltes før lagring.

