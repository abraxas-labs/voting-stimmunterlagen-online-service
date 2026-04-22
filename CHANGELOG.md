# ✨ Changelog (`v2.54.2`)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version Info

```text
This version -------- v2.54.2
Previous version ---- v2.54.1
Initial version ----- v2.5.1
Total commits ------- 1
```

## [v2.54.2] - 2026-04-21

### 🔄 Changed

- update test domain of influence config

## [v2.54.1] - 2026-04-08

### 🔄 Changed

- update test domain of influence config

## [v2.54.0] - 2026-04-02

### :arrows_counterclockwise: Changed

- VotingCardPrintFileBuilder: new docId DocIdContestWithAttchmentFormatA4LayoutA5 = "U4A5" to recognize template layouts with a5 including attachements a4

## [v2.53.2] - 2026-04-01

### :arrows_counterclockwise: Changed

- VotingCardPrintFileBuilder: get PrintData from snapshotted votingCardLayout

## [v2.53.1] - 2026-04-01

### 🔒 Security

- use latest lib to resolve vulnerabilities in Magick.NET

## [v2.53.0] - 2026-03-30

### :new: Added

- value to CustomerSubdivision in VotingCardFileBuilder

###

## [v2.52.0] - 2026-03-25

### 🆕 Added

- feat(VOTING-6623): stistat export

## [v2.51.2] - 2026-03-17

### 🔄 Changed

- update test domain of influence config

## [v2.51.1] - 2026-03-17

### :arrows_counterclockwise: Changed

- exclude voters with missing date of birth on is minor sync
- treat nuget vulnerabilty warnings as warning

## [v2.51.0] - 2026-03-05

### :arrows_counterclockwise: Changed

- VotingJournalVotingRenderService: splitted adress to street, postalcode and town

## [v2.50.3] - 2026-03-04

### 🆕 Added

- support eCH-0045 foreigner extensions

## [v2.50.2] - 2026-03-03

### 🔄 Changed

- add additional contest voting card layout builder log info

## [v2.50.1] - 2026-02-26

### :new: Added

- add newly introduced parameter to pass async job priority for start async pdf generation and use default

### :lock: Security

- update magick image processing library to latest non vulnerable version

## [v2.50.0] - 2026-02-18

### 🔄 Changed

- extend auto split to return address applications

## [v2.49.1] - 2026-02-18

### 🔄 Changed

- reorder metrics middleware calls in Startup configuration to catch final response status.

## [v2.49.0] - 2026-02-12

### Changed

- ContestVotingCardLayoutBuilder: synchronize PrintData with DomainOfInfluence unless print is done
- TemplateManager: take PrintData from Layout.
- VotingCardLayout model: added PrintDate to store Printdata per Layout

## [v2.48.0] - 2026-02-06

### 🆕 Added

- add eCH-0045 v6 import

## [v2.47.2] - 2026-02-06

### 🔄 Changed

- extend CD pipeline with enhanced bug bounty publication workflow

## [v2.47.1] - 2026-01-27

### 🆕 Added

- add attachment accessible political businesses

## [v2.47.0] - 2026-01-20

### 🆕 Added

- add canton AR

## [v2.46.1] - 2026-01-15

### 🔄 Changed

- fix voter duplicates with at least 3 occurrences

## [v2.46.0] - 2026-01-06

### :new: Added

- ReligionCode to VotingCardPrintFileBuilder

## [v2.45.1] - 2026-01-05

### 🔄 Changed

- update test domain of influence config

## [v2.45.0] - 2025-12-18

### :new: Added

- VoterTypeHelper class for GetYoterTypeSortValue function.
- VoterAsyncEnummerableExtensionTest for tests of street with number sorting and denomination sorting

## [v2.44.0] - 2025-12-15

### :new: Added

- DomainOfInfluenceResolver to map codes to VoterDomainOfInfluence List

### :arrows_counterclockwise: Changed

- Mapping of Religion Code according new Codes "E WR" and "K WR"

## [v2.43.4] - 2025-12-11

### 🔄 Changed

- include voter duplicate domain of influence infos on voting card print

## [v2.43.3] - 2025-12-11

### :arrows_counterclockwise: Changed

- function BuildEmtyVoters in EmtyVoterBuilder: added bfs for voter list.

## [v2.43.2] - 2025-12-10

### 🔄 Changed

- fix update ignore relations method

## [v2.43.1] - 2025-12-09

Included DomainOfInfluences for voter selecten where needed

## [v2.43.0] - 2025-12-09

### :new: Added

- new model VoterDomainOfInfluence
- DomainOfInfluenceIdentificationsChurch and DomainOfInfluenceIdentificationsSchool to TemplateDataBuilder including mapping in TemplateDataProfile
- mapping to VoterDomainOfInfluence from ECH-Import

## [v2.42.3] - 2025-12-05

### :new: Added

- default contstructor to VotingCardLayoutDataConfiguration

### :arrows_counterclockwise: Changed

- set all options in VotingCardLayoutDataConfiguration to false for empty voting cards in ManualVotingCardGeneratorJobManager and VotingCardPrintFileExportGenerator

## [v2.42.2] - 2025-12-04

### 🆕 Added

- DenominationHelper and HouseNumberHelper as dedicated helper class for ordering voter list.
- OrderBySortingCriteriaAsync method in AsyncEnumerableExtensions class

### 🔄 Changed

- switched OrderBy IQueriable async funktion to OrderBySortingCriteriaAsync IEnumerable async funktion in classes VotingCardGenerator, VotingCardGeneratorJobBuilder, VotingCardPrintFileExportGenerator, VotingJournalVotingRenderService. Ordering will be executed in memory and not on db due to database incompatibility of sorting functions.

## [v2.42.1] - 2025-12-04

### 🔄 Changed

- handle voting card job with empty voting cards correctly in the callback

## [v2.42.0] - 2025-12-02

### 🆕 Added

- add voter lists with empty voting cards

## [v2.41.3] - 2025-11-27

### 🔄 Changed

- set householder to false for voter without residence building id or apartment id

## [v2.41.2] - 2025-11-26

### 🆕 Added

- PostOfficeBoxMapping class: Handles the mapping of post office box information and provides a function to merge the post office box text and number into a single string.

### 🔄 Changed

- class VoterMapping: added mapping for post office box

## [v2.41.1] - 2025-11-25

### 🔄 Changed

- VoterQueryableExtensions: optimization of housenumber sorting for entity-framework

## [v2.41.0] - 2025-11-20

### 🆕 Added

- add empty voting cards per domain of influence flag

## [v2.40.5] - 2025-11-19

### 🔄 Changed

- extend voter duplicate with street and house number

## [v2.40.4] - 2025-11-19

### 🆕 Added

- add e-voting domain of influence config

## [v2.40.3] - 2025-11-18

### 🆕 Added

- add domain of influence bfs

## [v2.40.2] - 2025-11-18

### 🆕 Added

- ReligionConverter for mapping religion enum from manual voting card to religion code as string.

### 🔄 Changed

- removed mapping of isHousholder in ManualVotingCardGeneratorJobProfile
- new proto file version

## [v2.40.1] - 2025-11-13

### 🆕 Added

- add bfs to evoter by doi export

## [v2.40.0] - 2025-11-05

### 🆕 Added

- add new sort criteria in VoterQueryableExtensions for place, dominination, household

## [v2.39.1] - 2025-11-03

### 🔄 Changed

- update test domain of influence config

## [v2.39.0] - 2025-11-03

### 🔄 Changed

- VoterQueryableExtensions extend ThenBy extension with sorting HouseNumber as number

## [v2.38.0] - 2025-10-28

### 🔄 Changed

- set IncludePersonId and IncludeDateOfBirth default to true if domain of influence has StistatMunicipality Flag.

## [v2.37.2] - 2025-10-28

### 🔄 Changed

- update test domain of influence config

## [v2.37.1] - 2025-10-16

### 🔄 Changed

- update post templates to show elections correctly

## [v2.37.0] - 2025-10-03

### 🆕 Added

- isMinor Flag to Voters Model
- IsMinor() function in Utils/DatamatrixMapping for global use

### 🔄 Changed

- MapReligion() in Utils/DatamatrixMapping
- ContestBuilder recalculate isMinor Flag if contes date has changed
- VotingStatisticVotingRenderService: add SwissAndSwissAbroadMinorMale, SwissAndSwissAbroadMinorFemale, ForeignerMinorMale, ForeignerMinorFemale

## [v2.36.1] - 2025-10-01

### 🔄 Changed

- check voting card permission on update domain of influence settings

## [v2.36.0] - 2025-09-25

### 🆕 Added

- add version choice and eCH-0045 v6 to e-voting export

## [v2.35.1] - 2025-08-28

### 🔄 Changed

- bump voting lib version

## [v2.35.0] - 2025-08-25

### 🆕 Added

- add main voting cards domain of influence

## [v2.34.3] - 2025-08-21

### 🔄 Changed

- creation of dummy voter as fuction instead of a static object.

## [v2.34.2] - 2025-08-21

### 🆕 Added

- add voting card counts

## [v2.34.1] - 2025-07-31

### 🔄 Changed

- update test domain of influence config

## [v2.34.0] - 2025-07-25

### 🆕 Added

- add voting card layout data configuration

## [v2.33.3] - 2025-07-25

### 🔄 Changed

- added new proto version

## [v2.33.2] - 2025-07-23

### 🆕 Added

- add new shippment code in print file if voting card is sent to return address

## [v2.33.1] - 2025-07-11

### 🆕 Added

- add return address addition to e-voting template

## [v2.33.0] - 2025-06-23

### 🔄 Changed

- disable generate voting cards if not all political business are e-voting approved

## [v2.32.4] - 2025-06-20

### 🔄 Changed

- use callname as firstname for voters if available

## [v2.32.3] - 2025-06-06

### 🔄 Changed

- update test domain of influence config

## [v2.32.2] - 2025-06-05

### 🔄 Changed

- update test domain of influence config

## [v2.32.1] - 2025-06-04

### 🆕 Added

- add attachment stations to municipality

## [v2.32.0] - 2025-05-28

### 🆕 Added

- mapping for List<ContestSummary> and ContestSummary to Contest proto
- new Model ContestSummary
- GetLowestPrintJobStateFromContest funtion in PrintJobManager

### 🔄 Changed

- List endpoint in ContestService. Enhanced contest with lowest print job state if user has role of print job manager.

## [v2.31.0] - 2025-05-28

### 🔄 Changed

- refactor dockerfile and reduce cache layers
- consume base images from harbor proxy

### 🔒 Security

- introduce user id and group id to avoid random assignment
- use exec form to avoid shell interpretation

## [v2.30.4] - 2025-05-27

### 🔄 Changed

- assign auslandschweizer template to test domain of influence

## [v2.30.3] - 2025-05-16

### 🆕 Added

- add communal deadlines preview

## [v2.30.2] - 2025-05-06

### 🔄 Changed

- improve communal contest deadline modification validation

## [v2.30.1] - 2025-04-28

### 🔄 Changed

- allow non-standard delivery to post deadlines

## [v2.30.0] - 2025-04-28

### 🆕 Added

- Eventprocessors for PoliticalAssemblyPastLocked and PoliticalAssemblyArchived

### 🔄 Changed

- moved UpdateState() function to ContestBuilder

## [v2.29.7] - 2025-04-17

### 🔄 Changed

- build attachment required for voter lists count correctly for political assemblies

## [v2.29.6] - 2025-04-16

### 🔄 Changed

- remove timezone info and qr-code on e-voting voting card

## [v2.29.5] - 2025-04-15

### 🔄 Changed

- fix missing domain of influence post data

## [v2.29.4] - 2025-04-14

### 🔄 Changed

- ManualVotingCardGeneratorJobManager added dummy value for voter.Bfs in Create function

## [v2.29.3] - 2025-04-08

### ❌ Removed

- remove invoice position per delivered voting card if it is shipped to municipality without packaging

## [v2.29.2] - 2025-04-08

### 🔒 Security

- fix permission for print job generate voting cards triggered list

## [v2.29.1] - 2025-04-01

### 🆕 Added

- add invoice entries voting card shipping method filter

## [v2.29.0] - 2025-03-27

### 🆕 Added

- add domain of influence franking licence away number

## [v2.28.0] - 2025-03-27

### 🆕 Added

- add contest dates electoral register e-voting from and delivery to post

## [v2.27.0] - 2025-03-17

### 🆕 Added

- generate empty manual voting card

## [v2.26.2] - 2025-03-17

### 🔄 Changed

- update test domain of influence config

## [v2.26.1] - 2025-03-12

### ❌ Removed

- Enum Values Chamois and Gold in VotingCardColor

## [v2.26.0] - 2025-03-07

### 🆕 Added

- support voting exports for single voter lists

## [v2.25.1] - 2025-03-03

### ❌ Removed

- remove attachment stations from e-voting config

## [v2.25.0] - 2025-02-28

### 🔄 Changed

- update voter duplicate check

## [v2.24.6] - 2025-02-07

### 🔄 Changed

- update test domain of influence config

## [v2.24.5] - 2025-02-07

### 🔄 Changed

- use default dmdoc username when unauthenticated

## [v2.24.4] - 2025-02-06

### 🔄 Changed

- update test domain of influence config

## [v2.24.3] - 2025-02-05

### 🔄 Changed

- support voting card generator job file name with multiple bfs and e-voting

## [v2.24.2] - 2025-01-30

### 🔄 Changed

- show person id with 11 digits on eVoting templates

## [v2.24.1] - 2025-01-30

### 🔄 Changed

- show number of housevoters on attachment category if needed

## [v2.24.0] - 2025-01-29

### 🔄 Changed

- consider householders during import

## [v2.23.0] - 2025-01-28

### 🆕 Added

- send attachment only to householder

## [v2.22.1] - 2025-01-21

### 🔄 Changed

- set default e-voter voting card shipping method

## [v2.22.0] - 2025-01-20

### 🆕 Added

- EVoterByDoiOnContestRenderService

## [v2.21.4] - 2025-01-15

### 🔄 Changed

- Class TemplateDataBuilder method BuildVotingCardColor()

## [v2.21.3] - 2025-01-08

### 🔄 Changed

- Dateformat for tags "dd.MM.yyyy" instead of "d.M.yyyy"

## [v2.21.2] - 2025-01-06

### 🔄 Changed

- ManualVotingCardGeneratorJobManager set contest date for reprint to get the taged bricks

## [v2.21.1] - 2024-12-20

### 🔄 Changed

- function TagBricks(DomainOfInfluenceVotingCardLayout layout) use new dmdoc-lib endpoint to select active bricks for tagging

## [v2.21.0] - 2024-12-18

### 🆕 Added

- include user id in log output

### 🔄 Changed

- update minio lib and testcontainer according to latest operated version

## [v2.20.0] - 2024-12-18

### 🔄 Changed

- snapshotting of bricks

## [v2.19.2] - 2024-12-17

### 🆕 Added

- stistat for test domain of influences

## [v2.19.1] - 2024-12-16

### 🔄 Changed

- update test domain of influence config

## [v2.19.0] - 2024-12-11

### 🆕 Added

- domain of influence voting card flat rate owner

## [v2.18.3] - 2024-12-06

### 🔄 Changed

- prevent political business approval step reset on political business update

## [v2.18.2] - 2024-12-06

### 🔄 Changed

- add additional invoice position comment

## [v2.18.1] - 2024-11-29

### 🔄 Changed

- fix clean up tasks for political assemblies

## [v2.18.0] - 2024-11-26

### 🔄 Changed

- color scheme in TemplateDataBuilder for colored reports

## [v2.17.7] - 2024-11-25

### 🔄 Changed

- optimize SourceLink integration and use new ci/cd versioning capabilities
- prevent duplicated commit ids in product version, only use SourceLink plugin.
- extend .dockerignore file with additional exclusions

## [v2.17.6] - 2024-11-25

### 🔄 Changed

- ensure unique station on communal attachments

## [v2.17.5] - 2024-11-22

### 🔄 Changed

- add e-voting info to domain of influence

## [v2.17.4] - 2024-11-07

### 🔄 Changed

- update person id in e-voting template

## [v2.17.3] - 2024-11-07

### 🆕 Added

- add voting card print specific invoice positions

## [v2.17.2] - 2024-11-06

### 🔄 Changed

- add new templates for 12 additional e-voting participants

## [v2.17.1] - 2024-10-31

### :arrows_counterclockwise: Changed

- update eCH library

## [v2.17.0] - 2024-10-30

### 🔄 Changed

- VotingCardPrintFileBuilder: set duplex only for templates that contains "_duplex" in it's template internal name

## [v2.16.2] - 2024-10-30

### 🔄 Changed

- extend voting card and print file name with contest infos

## [v2.16.1] - 2024-10-23

### 🆕 Added

- polital assembly specific flatrate invoice position

## [v2.16.0] - 2024-10-18

### 🆕 Added

- add STISTAT municipality flag to domain of influence
- include STISTAT flag in e-voting export

## [v2.15.2] - 2024-10-16

### 🔄 Changed

- make contest approval revertable

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
