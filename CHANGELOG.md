# ✨ Changelog (`v2.15.1`)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version Info

```text
This version -------- v2.15.1
Previous version ---- v2.8.0
Initial version ----- v2.5.1
Total commits ------- 29
```

## [v2.15.1] - 2024-10-11

### 🔄 Changed

- optional ech-0045 voter extensions

## [v2.15.0] - 2024-10-08

### 🆕 Added

- voter list auto send voting cards to doi return address split

## [v2.14.1] - 2024-10-04

### 🔄 Changed

- fix e-voting printfiles and include all templates

## [v2.14.0] - 2024-10-02

### 🔄 Changed

- update DOI templates for new evoting municipalities according to customer feedback

## [v2.13.4] - 2024-10-02

### 🔄 Changed

- fix political assembly attachment stations in exports

## [v2.13.3] - 2024-09-30

### 🔄 Changed

- align logo and receiver address in the swiss abroad e-voting template

## [v2.13.2] - 2024-09-25

### 🔄 Changed

- update positioning of logo and receiver address in the swiss abroad e-voting template

## [v2.13.1] - 2024-09-24

### 🆕 Added

- add new DOI templates for 12 additional e-voting participants

## [v2.13.0] - 2024-09-19

### 🔄 Changed

- VotingCardPrintFileBuilder: if template contains "_a5" in internal name set DOCID to "V5A5" for political assembly and "U5A5" for contest. Set FORMULAR in both cases to "BL200"

## [v2.12.6] - 2024-09-18

### 🔄 Changed

- increase max length of attachment name and supplier

## [v2.12.5] - 2024-09-18

### ❌ Removed

- remove sync steps method
- remove xml extensions class

## [v2.12.4] - 2024-09-18

### 🔒 Security

- add is external printing center checks

## [v2.12.3] - 2024-09-17

### 🔒 Security

- ensure print job is not external priting center for additional invoice positions

## [v2.12.2] - 2024-09-17

### 🔒 Security

- add permission checks for voter list imports

## [v2.12.1] - 2024-09-16

### 🔄 Changed

- Mapping of birthdate from reprint in class ManualVotingCardGeneratorJobProfile

## [v2.12.0] - 2024-09-12

### 🔄 Changed

- consider testing phase in testDeliveryFlag

## [v2.11.7] - 2024-09-03

### 🔄 Changed

- fix image yaml naming

## [v2.11.6] - 2024-09-03

### 🔄 Changed

- migrate from gcr to harbor

## [v2.11.5] - 2024-08-28

### :arrows_counterclockwise: Changed

- update bug bounty template reference
- patch ci-cd template version, align with new defaults

## [v2.11.4] - 2024-08-22

### 🔄 Changed

- move environment specific app settings out of default file

## [v2.11.3] - 2024-08-22

### 🔄 Changed

- encode voting card pdf callback url

## [v2.11.2] - 2024-08-21

### 🔄 Changed

- disable mock services in release build

## [v2.11.1] - 2024-08-20

### 🔄 Changed

- ensure swagger generator can be disabled completely

## [v2.11.0] - 2024-08-19

### 🔄 Changed

- apply CORS allowed origin least privilege

## [v2.10.2] - 2024-08-15

### 🆕 Added

- function WhereBelongToDomainOfInfluenceOnlyVoterList in class QueryableExtensions to select only voters from importet lists

### 🔄 Changed

- replaced WhereBelongToDomainOfInfluence with WhereBelongToDomainOfInfluenceOnlyVoterList  in Render function of classes StatisticsByReligionVotingRenderService, VotingJournalVotingRenderService and VotingStatisticsVotingRenderService

## [v2.10.1] - 2024-08-08

### 🔄 Changed

- Updated the VotingLibVersion property in the Common.props file from 12.10.2 to 12.10.5. This update includes improvements for the proto string validation for better error reporting.

## [v2.10.0] - 2024-07-22

### 🔄 Changed

- set brickversion to null in VotingCardManager and VotingCardGeneratur

## [v2.9.1] - 2024-07-19

### 🔄 Changed

- use domain of influence to build voting card file names

## [v2.9.0] - 2024-07-18

### 🆕 Added

- New Datamapping MapPersonId(string personId) to DatamatrixMapping
- person id mapping to VotingJournalVotingRenderService and TemplateDataProfile

### 🔄 Changed

- Used new fuction in ManualVotingCardGeneratorJobManager for person id padding

## [v2.8.0] - 2024-07-16

### 🔄 Changed

- set counting circle e-voting at a specific date

## [v2.7.1] - 2024-07-15

### 🔒 Security

- upgrade npgsql to fix vulnerability CVE-2024-0057

## [v2.7.0] - 2024-07-11

### 🆕 Added

- extend filter metadata with person actuality indicators

## [v2.6.3] - 2024-07-11

### 🆕 Added

- add additional print job details

## [v2.6.2] - 2024-07-10

### 🔒 Security

- add permission check on template bricks

## [v2.6.1] - 2024-07-05

### 🔒 Security

- add restriction for import and data section content types.

## [v2.6.0] - 2024-07-05

### 🔄 Changed

- JobData Class: deleted property JobId and CallbackUrl (no more used properties -> code cleanup), added property BricksVersion to send contest date to ensure selecting the bricks belonging to the contest in dmdoc.
- function BuildBag in TemplateDataBuilder class: new parameter contestDate / set to null if preview for template selection, deleted parameter jobId

## [v2.5.2] - 2024-07-04

### 🔄 Changed

- update voting library to implement case-insensitivity for headers as per RFC-2616

## [v2.5.1] - 2024-07-03

### 🎉 Initial release for Bug Bounty
