# ✨ Changelog (`v2.24.6`)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version Info

```text
This version -------- v2.24.6
Previous version ---- v2.15.1
Initial version ----- v2.5.1
Total commits ------- 37
```

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

### 🔒 Security

- add permission checks for voter list imports

### 🔄 Changed

- Mapping of birthdate from reprint in class ManualVotingCardGeneratorJobProfile

### 🔄 Changed

- consider testing phase in testDeliveryFlag

### 🔄 Changed

- fix image yaml naming

### 🔄 Changed

- migrate from gcr to harbor

### :arrows_counterclockwise: Changed

- update bug bounty template reference
- patch ci-cd template version, align with new defaults

### 🔄 Changed

- move environment specific app settings out of default file

### 🔄 Changed

- encode voting card pdf callback url

### 🔄 Changed

- disable mock services in release build

### 🔄 Changed

- ensure swagger generator can be disabled completely

### 🔄 Changed

- apply CORS allowed origin least privilege

### 🆕 Added

- function WhereBelongToDomainOfInfluenceOnlyVoterList in class QueryableExtensions to select only voters from importet lists

### 🔄 Changed

- replaced WhereBelongToDomainOfInfluence with WhereBelongToDomainOfInfluenceOnlyVoterList  in Render function of classes StatisticsByReligionVotingRenderService, VotingJournalVotingRenderService and VotingStatisticsVotingRenderService

### 🔄 Changed

- Updated the VotingLibVersion property in the Common.props file from 12.10.2 to 12.10.5. This update includes improvements for the proto string validation for better error reporting.

### 🔄 Changed

- set brickversion to null in VotingCardManager and VotingCardGeneratur

### 🔄 Changed

- use domain of influence to build voting card file names

### 🆕 Added

- New Datamapping MapPersonId(string personId) to DatamatrixMapping
- person id mapping to VotingJournalVotingRenderService and TemplateDataProfile

### 🔄 Changed

- Used new fuction in ManualVotingCardGeneratorJobManager for person id padding

### 🔄 Changed

- set counting circle e-voting at a specific date

### 🔒 Security

- upgrade npgsql to fix vulnerability CVE-2024-0057

### 🆕 Added

- extend filter metadata with person actuality indicators

### 🆕 Added

- add additional print job details

### 🔒 Security

- add permission check on template bricks

### 🔒 Security

- add restriction for import and data section content types.

### 🔄 Changed

- JobData Class: deleted property JobId and CallbackUrl (no more used properties -> code cleanup), added property BricksVersion to send contest date to ensure selecting the bricks belonging to the contest in dmdoc.
- function BuildBag in TemplateDataBuilder class: new parameter contestDate / set to null if preview for template selection, deleted parameter jobId

### 🔄 Changed

- update voting library to implement case-insensitivity for headers as per RFC-2616

### 🔄 Changed

- correct print job list permission check

### 🆕 Added

- add templates to e-voting export dynamically

### ❌ Removed

- remove deprecated and unused code

### 🔄 Changed

- proto reference to fix proto-validation of bricks

### 🆕 Added

- add firstname and lastname to voting journal

### 🔄 Changed

- Move base e-voting zip to object storage

### 🆕 Added

- proto validation

### 🔒 Security

- allow get attachments progress only per contest manager

### ❌ Removed

- remove deprecated role dmdoc

### 🆕 Added

- bug bounty integration

### 🆕 Added

- extend ci/cd pipeline with bbt environment
- add swagger api specification generation

### 🔄 Changed

- moved dummy DomainOfInfluence to TemplateDataBuilder

### 🔄 Changed

- added dummy domain of influence for tenants without. On GetPdfPreview in TemplateManager

### 🔄 Changed

- resolve bugs and code smells reported by sonarqube.

### 🔄 Changed

- Voting Lib Reference to enable priority print jobs on dmdoc for preview

### 🔄 Changed

- configure certificate pinning
- use secure https communication for CONNECT delivery

### 🆕 Added

- recursive function to get child categories. Fix to select only the relevant dmdoc templates for tennant.

### 🔄 Changed

- move Stimmregister flag from canton settings to DOI

### 🔄 Changed

- adjust e-voting voting cards

BREAKING CHANGE: Updated service to .NET 8 LTS.

### :arrows_counterclockwise: Changed

- update to dotnet 8

### :lock: Security

- apply patch policy

### :new: Added

- added voting card color

### 🔄 Changed

- apply change requests for e-voting voting cards

### 🆕 Added

- add post office box text to voter
- new e-voting templates

### 🆕 Added

- add political assembly

### 🔄 Changed

- domain of influence sync with canton and canton defaults

### 🆕 Added

- add evoting counting circle

### 🆕 Added

- add additional voter statistic export columns

### 🔄 Changed

- add text templates and test urns for 11 new e-voting municipalities

### 🔄 Changed

- anonymous authorization for dmdoc callback

### 🔄 Changed

- adjustments voter list import and voting journal

### 🔄 Changed

- add date of birth to voters in voting journal export

### 🔄 Changed

- change file names of eCH exports

### 🆕 Added

- voting journal

### 🔄 Changed

- change order of businesses in attachments step

### 🔄 Changed

- voter person id optional

### 🔄 Changed

- Update basis proto dependency

### 🆕 Added

- database query monitoring

### 🆕 Added

- Add bfs 8405 and 8215 to e-voting

### 🔄 Changed

- E-voting doi config template text

### 🔄 Changed

- update e-voting templates

### 🔄 Changed

- Update e-voting templates

### 🆕 Added

- add eCH from voting lib

### :arrows_counterclockwise: Changed

- improve voter pages logging

### 🔄 Changed

- delegate draft cleanup to background job by enqueuing it to cleanup queue
- schedule draft content cleanup after successful callback
- schedule hard draft cleanup for failed generation jobs

### :arrows_counterclockwise: Changed

- replace voter pages webhook with call to DocPipe

### :arrows_counterclockwise: Changed

- use separate port for metrics endpoint provisioning

### 🔄 Changed

- Set print job state correctly on sync

### :arrows_counterclockwise: Changed

- add additional oauth client scopes for subsystem access authorization

### 🔄 Changed

- Manual voting card voter with numeric person id

### :new: Added

- add support for custom oauth scopes.

### 🆕 Added

- Additional invoice position with material

### 🔄 Changed

- Voter list improvements

### 🔄 Changed

- Validate correct number in expected number of voters validation on electoral register uploads

### 🔄 Changed

- update lib to add dmdoc callback retry

### 🆕 Added

- Auto split voter lists

### 🔄 Changed

- use latest lib to resolve Magick security issue

### 🔒 Security

- use latest lib to resolve Magick security issue

### 🔄 Changed

- Load print jobs even if contest is locked

### 🔄 Changed

- improve voting card generator job memory usage

### 🆕 Added

- Add invoice material envelope positions

### 🔄 Changed

- Adjust invoice material numbers

### 🔄 Changed

- set default auth if not authenticated in background job

### 🔄 Changed

- Deliver print file with contest order number

### 🔄 Changed

- Moved contest order number config to event processor config

### 🆕 Added

- Added contest order number
- Added voter index in a contest

### 🔄 Changed

- Update eai and lib dependency to deterministic version

### ❌ Removed

- remove swiss post order number

### ❌ Removed

- remove attachment station unique per level restriction

### 🔄 Changed

- Add mocked shipment number to voter

### 🆕 Added

- add swiss post data

### 🔄 Changed

- Migrate optional owned domain of influence print data correctly

### 🔄 Changed

- prevent invalid dois in e-voting export

### 🔄 Changed

- Invoice

### 🔄 Changed

- Map voter address according to domain of influence print data flag

### 🔄 Changed

- Improve e-voting domain of influence logo quality

### 🆕 Added

- get electoral register filter metadata api

### 🆕 Added

- add count of invalid persons in electoral register filter version

### 🆕 Added

- Test domain of influence logos and text blocks

### 🆕 Added

- add config to allow unsafe use of insecure channel call credentials to allow cluster internal grpc traffic without tls

### 🔄 Changed

- Manual voting card voter with person id and date of birth

### 🔄 Changed

- migrate databases only when eventprocessor mode is enabled

### 🔄 Changed

- rm unneeded appsettings service account scopes

### 🔄 Changed

- propagate http headers to stimmregister

### 🔄 Changed

- Get random voter samples in voting card pdf preview
- Updated EVoting Templates

### 🆕 Added

- add voter list with new electoral register filter version

### 🆕 Added

- add voter list import via Stimmregister

### ❌ Removed

- remove certificate for evoting export

### 🆕 Added

- Invoice

### 🔄 Changed

- add domain of influence settings api

### 🔄 Changed

- apply canton settings to domain of influences

### 🆕 Added

- store canton settings ElectoralRegistrationEnabled

### 🆕 Added

- Added attachment stations to E-Voting export

### 🔄 Changed

- add text blocks for e-voting export

### 🆕 Added

- add voting cards shipment weight

### 🆕 Added

- Attachment category voting guide

### ❌ Removed

- remove voting cards sent count

### 🔄 Changed

- exclude domain of influences without evoting

### 🆕 Added

- add priting certificates

### 🔄 Changed

- change to default duplex print

### 🔄 Changed

- change evoting export templates for non auslandschweizer

### 🆕 Added

- add test counting circles for evoting export

### 🔄 Changed

- Fixed handling with E-Voting voting card generator jobs

### ❌ Removed

- E-Voting layouts

### 🔄 Changed

- evoting configuration file shipping method mapping

### 🔄 Changed

- move transaction util to voting lib

### 🔄 Changed

- Set service user as authorized user in dispatched print file export context

### 🆕 Added

- add scoped dmdoc httpclient

### 🔄 Changed

- update basis eventing proto

### ❌ Removed

- remove internal description from political business

### 🔄 Changed

- Set correct return delivery type for offline client

### 🔄 Changed

- Provided additional role debug infos

### 🔄 Changed

- Added voter id to template data

### 🆕 Added

- Added deliver to domain of influence return address option to voter list

### 🔄 Changed

- Set auth from request on dispatched voting card generate job

### 🔄 Changed

- add print job detail page for election admin

### 🆕 Added

- Added handling of the DmDoc voting card generator job callback
- Added voter page infos

### 🔄 Changed

- Update contest domain of influence counting circles correctly

### 🔄 Changed

- Moved template data field values from domain of influence to layout

### 🆕 Added

- Added attachment category

### 🆕 Added

- add domain of influence voting card print data

### 🔄 Changed

- Set attachment counts depending on the domain of influence type

### 🔄 Changed

- Enabled attachment step for domain of influences with external printing center

### 🆕 Added

- Exclude child domain of influences in attachments

### 🔄 Changed

- Use tenant specific template bricks

### 🆕 Added

- Added contest e-voting export file hash

### 🆕 Added

- Added voting card print file export

### 🔄 Changed

- Extended eCH-0045 mapping

### 🔄 Changed

- Added additional fields in the update brick content response

### 🔄 Changed

- Updated dependencies

### 🆕 Added

- Added domain of influence voting card bricks

### 🔄 Changed

- Sorted political businesses by ascending domain of influence type

### 🔄 Changed

- Correctly reset print job state after contest deadline is postponed

### 🆕 Added

- Added contest generate voting cards deadline

### 🔄 Changed

- Domain of influence readyness for the e-voting export

### 🔄 Changed

- Added parent political businesses to attachments

### 🆕 Added

- add application builder extension which is adding the serilog request logging middleware enriching the log context with tracability properties
- add Serilog.Expressions to exclude status endpoints from serilog request logging on success only

### 🔄 Changed

- fix possible null reference within app module interceptor

### 🔒 Security

- update nuget packages

### 🔄 Changed

- exchanged custom health check with ef core default one

### 🆕 Added

- Add attachment format a6

### 🔄 Changed

- exchanged ef core default health check with custom one

### 🔄 Changed

- Contests merge processing

### 🔄 Changed

- compressed domain of influence logo on e-voting export

### 🆕 Added

- external printing center

### 🆕 Added

- accumulated eCH-0045 in E-Voting Export and eCH-0045 serializer

### 🆕 Added

- contest E-Voting

### 🆕 Added

- CORS configuration support

### 🔄 Changed

- set contest deadlines per date

### 🔄 Changed

- upgraded underlying dotnet image to sdk 6.0.301 after gituhb issue [#24269](https://github.com/dotnet/sdk/issues/24269) has been fixed

Prefix the secure connect tenant id with 'tenantId_' due to dmDoc category lookup restrictions.
Also ignore 404 since dmDoc returns them, if either the category is unknown or the list is empty.
However since this can happen a lot in our usecases we simply map 404 to an empty template list for this use-case.

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
