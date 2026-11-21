# Knowledge Base – Database Schema & Business Logic

## Available Tables

The following tables exist in the database. Use only these tables when building queries.

| Table | Description |
|---|---|
| `tbl_Tags` | Products currently in the warehouse (inventory items / RFID tags) |
| `tbl_Destination` | Warehouses and warehouse hierarchy |
| `tbl_DestinationType` | Types and categories of warehouses/destinations |
| `tbl_Zones` | Locations / storage zones inside a warehouse |
| `tbl_ProductBrand` | Product brands |
| `tbl_ProductGroup` | Product groups |
| `tbl_ProductSubGroup` | Product sub-groups |
| `tbl_ProductClass` | Product classes |
| `tbl_ProductStatus` | Product quality/status codes |
| `tbl_ProductPropertyA` | Product property dimension A |
| `tbl_ProductPropertyB` | Product property dimension B (linked to A) |
| `tbl_ProductPropertyC` | Product property dimension C |
| `tbl_ProductType` | Product types and categories |
| `tbl_MovementActions` | History of all movements, operations, and transactions (Header) |
| `tbl_TagsMovement` | Details of tags moved in a transaction (Line items) |
| `tbl_ActionTypes` | Types of operations (e.g., Entry, Exit, Sale) |
| `tbl_Station` | Gates or stations where RFID logs are recorded |
| `tbl_User` | System users and operators |

### Key Column Reference

**`tbl_Tags`** (main inventory table)
- `ProductSerial`, `TagEpc` – primary key
- `ProductCode`, `ProductName`, `ProductType`
- `ProductStatus` – quality/status title
- `ProductCount` – quantity
- `TagInDestinationId` – warehouse code (FK → `tbl_Destination.DestinationCode`)
- `TagZone` – zone code (FK → `tbl_Zones.ZoneCode`)
- `TagRegisterDateTime` – registration datetime
- `TagRegisterShamsiUnixDate` – Shamsi (Persian) date as Unix timestamp
- `TagStatus` – tag lifecycle status
- `Lock`, `Freeze`, `Deactivate` – boolean flags. (Only filter by `Deactivate = 0` if the user explicitly says "فعال", or `Deactivate = 1` for "غیرفعال". If not mentioned, do not filter).
- `fld_ProductPropertyAId`, `fld_ProductPropertyBId`, `fld_ProductPropertyCId`
- `fld_ProductGroup`, `fld_ProductBrand`, `fld_ProductSubGroup`, `fld_ProductClass`
- `TagRegisterUser`, `Username`, `fld_LastModifierUser`
- `ProductType` – Product type code (FK → `tbl_ProductType.ProductTypeCode`)

**`tbl_Destination`** (warehouses)
- `DestinationId`, `DestinationCode`, `DestinationTitle`
- `DestinationType`, `DestinationParentId` – hierarchy

**`tbl_Zones`** (locations)
- `ZoneCode`, `ZoneTitle`, `ZoneStoreCode` – warehouse code
- `ZoneCapacity`, `ZoneOccupiedCapacity`

**`tbl_MovementActions`** (Transaction Header)
- `MovementActionId` – Primary Key
- `MovementActionStore` – Source warehouse code (FK → `tbl_Destination.DestinationCode`)
- `MovementActionDestinationId` – Target warehouse code (FK → `tbl_Destination.DestinationCode`)
- `MovementActionTp` – Action type code (FK → `tbl_ActionTypes.fld_ActionTypeId`)
- `MovementActionUserId` – User ID (FK → `tbl_User.Id`)
- `MovementActionUHFLogGate` – Station/Gate code (FK → `tbl_Station.fld_StationCode`)
- `MovementActionDate`, `MovementActionTime`, `MovementActionDesc`

**`tbl_TagsMovement`** (Transaction Details)
- `TagsMovementId` – Primary Key
- `RMovementActionId` – FK to `tbl_MovementActions.MovementActionId`
- `ProductSerial`, `ProductCode`
- `ProductCount` – quantity moved

**`tbl_ActionTypes`, `tbl_Station`, `tbl_User`** (Lookups)
- `tbl_ActionTypes`: `fld_ActionTypeId`, `fld_ActionTypeTitle`
- `tbl_Station`: `fld_StationCode`, `fld_StationName`
- `tbl_User`: `Id`, `Name`, `Username`

**`tbl_Products`** (Master Product Data)
- `ProductCode` – Primary Key
- `ProductTitle`, `ProductTechnicalCode`, `ProductSize`

**`tbl_DestinationType`** (Warehouse Types)
- `fld_DestinationTypeCode` – Primary Key
- `fld_DestinationTypeName` – Type Name/Title
*(Note: `tbl_Destination.DestinationType` is the FK to `tbl_DestinationType.fld_DestinationTypeCode`)*

### Foreign Key & Lookup Table Mapping (CRITICAL FOR FILTERING)
When a user wants to filter by a name or title (e.g., Warehouse, Zone, Product, Size, Line, Shift), you MUST use this mapping to JOIN the correct table and filter on the Title/Name column using `LIKE`.
**NEVER** compare a user's Persian text directly against a code/ID column in `tbl_Tags`.

| Entity Name (User asks for...) | FK in `tbl_Tags` | Target Table | Join Condition | Filter Column (Use LIKE) |
|---|---|---|---|---|
| Warehouse / Destination | `TagInDestinationId` | `tbl_Destination` | `DestinationCode` | `DestinationTitle` |
| Zone / Location | `TagZone` | `tbl_Zones` | `ZoneCode` | `ZoneTitle` |
| Master Product | `ProductCode` | `tbl_Products` | `ProductCode` | `ProductTitle` |
| Brand | `fld_ProductBrand` | `tbl_ProductBrand` | `fld_ProductBrandCode` | `fld_ProductBrandTitle` |
| Group | `fld_ProductGroup` | `tbl_ProductGroup` | `fld_ProductGroupCode` | `fld_ProductGroupTitle` |
| SubGroup | `fld_ProductSubGroup` | `tbl_ProductSubGroup` | `fld_ProductSubGroupCode` | `fld_ProductSubGroupTitle` |
| Class | `fld_ProductClass` | `tbl_ProductClass` | `fld_ProductClassCode` | `fld_ProductClassTitle` |
| Production Line (Property A) | `fld_ProductPropertyAId`| `tbl_ProductPropertyA`| `fld_ProductPropertyAId` | `fld_ProductPropertyATitle` |
| Work Shift (Property B) | `fld_ProductPropertyBId`| `tbl_ProductPropertyB`| `fld_ProductPropertyBId` | `fld_ProductPropertyBTitle` |
| Size (Property C) | `fld_ProductPropertyCId`| `tbl_ProductPropertyC`| `fld_ProductPropertyCId` | `fld_ProductPropertyCTitle` |
| Quality Status | `ProductStatus` | `tbl_ProductStatus` | `ProductStatusCode` | `ProductStatusTitle` |
| Product Type / Category | `ProductType` | `tbl_ProductType` | `ProductTypeCode` | `ProductTypeTitle` |
| Action Type / Operation | `MovementActionTp` | `tbl_ActionTypes` | `fld_ActionTypeId` | `fld_ActionTypeTitle` |
| Station / Gate | `MovementActionUHFLogGate` | `tbl_Station` | `fld_StationCode` | `fld_StationName` |
| User / Operator | `MovementActionUserId` | `tbl_User` | `Id` | `Name` |
| Warehouse Type (نوع انبار) | `DestinationType` | `tbl_DestinationType` | `fld_DestinationTypeCode` | `fld_DestinationTypeName` |

#### 1. Entity Relationships and Data Sources

The database contains several functional areas, each represented
by dedicated tables.

##### 1.1 Master Product Data

tbl_Products contains master product definitions and technical
information, including product titles, technical codes, sizes,
and dynamic technical properties stored in ProductTechnicalData.

##### 1.2 Current Inventory and Registered Tags

tbl_Tags represents currently registered inventory items and
their associated RFID/tag information.

It contains product serials, EPC values, product information,
warehouse and zone references, registration information,
lifecycle status, and inventory-related properties.

##### 1.3 Entry History

tbl_TagsMovement stores the details of inventory movements.

The RMovementActionId relationship connects movement details
to the corresponding operation in tbl_MovementActions.

This relationship represents the entry/registration movement
path in the existing Silo business logic.

##### 1.4 Exit and Return History

The HMovementActionId relationship connects movement details
to operations in tbl_MovementActions through the
exit/return movement path.

The actual business meaning of each operation is represented
by MovementActionTp and its corresponding record in
tbl_ActionTypes.

##### 1.5 Operations and Movements

tbl_MovementActions contains operation-level information,
including operation type, associated warehouses, operator,
station/gate, date, time, description, and dynamic operation
properties.

tbl_TagsMovement contains the individual product and serial
details associated with these operations.

##### 1.6 RFID Reader Logs

tbl_UHF_ReaderLog contains RFID detection records generated
by readers, antennas, and gates.

These records represent physical tag detection events rather
than inventory registration or warehouse movement records.

The relationship between RFID logs and inventory tags is:

tbl_UHF_ReaderLog.fld_TagSerial
→ tbl_Tags.TagEpc

##### 1.7 Truck and Vehicle Crossings

tbl_TruckCross contains truck crossing information, including
vehicle-related records.

Operation-related truck crossing information is associated
with tbl_MovementActions through MovementActionTruckCrossId.

##### 1.8 Handheld Inventory Counting

tbl_InventoryTags contains records associated with manual
inventory counting performed using handheld devices.

Its relationship with inventory tags is:

tbl_InventoryTags.fld_InventoryTagEPC
→ tbl_Tags.TagEpc

##### 1.9 Physical Counting

Physical counting (شمارش فیزیکی) is a separate business
process from handheld inventory counting (انبارگردانی دستی).

Physical counting may involve RFID reader devices and
Excel-based processes.

##### 1.10 Product Placement

tbl_ProductPlacementMissions contains records related to
product/tag placement and movement between storage zones.

Its relationship with registered inventory is:

tbl_ProductPlacementMissions.fld_PPMProductSerial
→ tbl_Tags.ProductSerial

#### 2. Movement History Relationships

Movement history is represented through the relationship
between inventory tags, movement details, and operation headers.

The main relationship is:

tbl_Tags
→ ProductSerial
→ tbl_TagsMovement
→ RMovementActionId / HMovementActionId
→ tbl_MovementActions
→ MovementActionTp
→ tbl_ActionTypes

##### 2.1 Movement Reference Paths

The Silo database uses two distinct movement reference fields:

- RMovementActionId:
  Represents the entry/registration movement path.

- HMovementActionId:
  Represents the exit/return movement path.

These fields identify different relationships between movement
details and operation headers.

##### 2.2 Operation Classification

The operation type is stored in:

tbl_MovementActions.MovementActionTp

The corresponding operation title is stored in:

tbl_ActionTypes.fld_ActionTypeTitle

HMovementActionId represents a movement-history relationship
and does not independently identify the physical nature of
an operation.

The operation's business classification is determined by
its associated ActionType and, where applicable, the dynamic
data stored in MovementActionData.

#### 3. Location Data Model

The Silo database distinguishes between physical locations
and location categories.

##### 3.1 Physical Locations

tbl_Destination represents actual warehouses and destinations.

Its main fields include:

- DestinationId
- DestinationCode
- DestinationTitle
- DestinationType
- DestinationParentId

DestinationCode identifies the business location and is used
by other tables as a warehouse reference.

##### 3.2 Location Types

tbl_DestinationType contains the classification of physical
locations.

Its fields include:

- fld_DestinationTypeCode
- fld_DestinationTypeName

The relationship between these entities is:

tbl_Destination.DestinationType
→ tbl_DestinationType.fld_DestinationTypeCode

##### 3.3 Operation Location References

tbl_MovementActions contains two location-related fields:

- MovementActionStore:
  References DestinationCode in tbl_Destination and represents
  the store/location associated with the operation.

- MovementActionDestinationId:
  References DestinationCode in tbl_Destination and represents
  the associated destination/target location.

The business meaning of source and target locations depends
on the operation type.

##### 3.4 Movement Location Types

tbl_ActionTypes contains two fields describing the location
types associated with an operation:

- fld_ActionTypeFromDestinationType:
  Represents the source location type.

- fld_ActionTypeToTypeDestinationType:
  Represents the target location type.

These fields are conceptually related to
tbl_DestinationType.

Together, ActionTypes, DestinationType, Destination, and
MovementActions describe the relationship between operation
types and physical source/target locations.

#### 4. Dynamic JSON Properties

Some database entities store additional business properties
in JSON-formatted columns instead of dedicated relational
columns.

These properties represent flexible attributes associated
with inventory items, operations, and master products.

##### 4.1 Tag / Inventory Properties

Source:
tbl_Tags.ProductProperties

Contains dynamic properties associated with registered
inventory items.

Individual JSON properties can be accessed using:

JSON_VALUE(t.ProductProperties, N'$."FieldName"')

##### 4.2 Operation Properties

Source:
tbl_MovementActions.MovementActionData

Contains additional dynamic information associated with
movement and operation records.

Individual JSON properties can be accessed using:

JSON_VALUE(ma.MovementActionData, N'$."FieldName"')

##### 4.3 Master Product Technical Properties

Source:
tbl_Products.ProductTechnicalData

Contains dynamic technical attributes associated with
master product definitions.

Individual JSON properties can be accessed using:

JSON_VALUE(p.ProductTechnicalData, N'$."FieldName"')

The JSON property names and their business meanings depend
on the relevant product, tag, or operation configuration.

#### 5. AGGREGATION RULES — CRITICAL
Do NOT confuse the number of unique tagged items with the total quantity.
If the user asks:
- "How many unique items/tags/serials?"
- "تعداد کالا"
- "تعداد سریال"
- "چند تگ داریم؟"
Use:
`COUNT(DISTINCT ProductSerial)`
Typically:
`COUNT(DISTINCT tm.ProductSerial)`
This returns the number of unique serials/items.

If the user asks:
- "What is the total quantity?"
- "مجموع موجودی"
- "مجموع تعداد کالا"
- "چه مقدار کالا داریم؟"
Use:
`SUM(ProductCount)`
Typically:
`SUM(t.ProductCount)`
This returns the sum of the quantity stored in `ProductCount`.
These two metrics are NOT interchangeable.

#### 6. Date and Time Data Model

The Silo database contains date and time information in
different formats depending on the entity and business process.

##### 6.1 Shift-Based Business Day

The Silo WMS uses shift-based business-day definitions.

Shift information is stored in tbl_ProductPropertyB.

Relevant fields include:

- fld_ProductPropertyBTitle
- fld_ProductPropertyBDesc

Shift descriptions may contain time intervals in the
HH:mm-HH:mm format.

The start and end times of these intervals define the
operational boundaries of each shift.

Consequently, the beginning of a business day may differ
from 00:00 depending on the configured shift definitions.

##### 6.2 Persian and Gregorian Date Conversion

The database uses the following functions for date conversion:

- dbo.JalaliDateToGeorgianDate:
  Converts a Persian/Shamsi date into its Gregorian equivalent.

- dbo.GeorgianDateToJalaliDate:
  Converts a Gregorian date into its Persian/Shamsi equivalent.

These functions serve different purposes in date processing.

##### 6.3 Gregorian Date Storage

Some database date and datetime columns store Gregorian values.

Examples include registration dates, operation-related dates,
RFID log dates, placement dates, and inventory dates,
depending on their actual column definitions.

The function dbo.GeorgianDateToJalaliDate converts these
values into Shamsi/Jalali representation.

##### 6.4 Shamsi-Native Date Storage

Some columns already store Persian/Shamsi dates.

A known example is:

tbl_MovementActions.MovementActionDate

This column stores dates directly in Shamsi/Jalali format.

Example values:

- 1405/02/10
- 1405/02/11

Its stored values are already suitable for Shamsi date
display, grouping, and chart categories.

##### 6.5 Shamsi Unix Timestamp

tbl_Tags.TagRegisterShamsiUnixDate stores registration
date information as a Shamsi Unix timestamp.

This representation differs from ordinary Gregorian
datetime values and from Shamsi date strings.

##### 6.6 Date and Time Representation

Date information may be stored separately from time
or together in a single datetime value.

When stored separately, the date and time are represented
by independent fields.

When stored together, both components are part of the
same datetime value.

Date storage format and display format are separate
characteristics of the database fields.

#### 7. CORE RELATIONSHIP MAP
The main database relationship chain is:
`tbl_Products`
→ `ProductCode`
→ `tbl_Tags`
→ `ProductSerial`
→ `tbl_TagsMovement`
→ `RMovementActionId / HMovementActionId`
→ `tbl_MovementActions`
→ `MovementActionTp`
→ `tbl_ActionTypes`

Location relationship:
`tbl_MovementActions`
→ `MovementActionStore / MovementActionDestinationId`
→ `tbl_Destination`
→ `DestinationType`
→ `tbl_DestinationType`

RFID relationship:
`tbl_Tags.TagEpc` ↔ `tbl_UHF_ReaderLog.fld_TagSerial`

Truck relationship:
`tbl_MovementActions.MovementActionTruckCrossId`
→
`tbl_TruckCross.fld_TruckCrossId`

Placement relationship:
`tbl_ProductPlacementMissions.fld_PPMProductSerial`
→
`tbl_Tags.ProductSerial`

Inventory relationship:
`tbl_InventoryTags.fld_InventoryTagEPC`
→
`tbl_Tags.TagEpc`

#### 8. QUERY GENERATION RULE
Before writing any SQL query:
1. Identify exactly what the user is asking for.
2. Select the correct base table using ENTITY RESOLUTION.
3. Identify whether the question is about:
   - Master Product
   - Tag / Current Inventory
   - Entry
   - Exit
   - Return
   - Movement / Operation
   - RFID Read
   - Placement
   - Inventory Counting
   - Truck Crossing
4. Follow the correct relationship chain.
5. Verify the meaning of `MovementActionTp` through `tbl_ActionTypes`.
6. For location questions, distinguish between:
   - Actual location (`tbl_Destination`)
   - Location type (`tbl_DestinationType`)
7. For dynamic fields, use `JSON_VALUE`.
8. For quantity questions, distinguish between:
   - Unique serial count → `COUNT(DISTINCT ProductSerial)`
   - Total quantity → `SUM(ProductCount)`
9. For date-based reports, respect shift boundaries and do not blindly use `00:00`.
10. Do not invent relationships or column meanings that are not supported by the database schema or known business logic.
The goal is not merely to generate syntactically valid SQL. The goal is to generate SQL that correctly represents the Silo WMS business logic.