# Silo WMS Database Knowledge Base
## Database Schema, Relationships, Business Logic, and Query Semantics

## 1. Purpose and Scope

This document describes the Silo WMS database structure, the relationships between its tables, the business meaning of important columns, and the rules required to interpret database records correctly.

The purpose of this knowledge base is to enable an AI agent to understand the database before generating SQL queries.

It serves as the reference for:

- Identifying the correct tables for a user's request.
- Understanding table relationships and valid join paths.
- Distinguishing current inventory from historical movements and master product data.
- Understanding warehouse locations, operation types, RFID records, inventory counting, and placement operations.
- Correctly interpreting quantities, dates, dynamic JSON properties, and operational history.
- Preventing incorrect assumptions about column meanings or business processes.

**Important:** This document describes database knowledge and established business semantics. It does not, by itself, authorize modifying data. Any future query-generation instructions must separately define the permitted SQL operations. For read-only reporting, use `SELECT` queries only.

### 1.1 Source of Truth and Confidence Levels

When interpreting the database, distinguish between:

1. **Schema facts:** Columns, data types, primary keys, identity properties, nullability, and constraints established by the actual SQL Server schema.
2. **Known business relationships:** Relationships and meanings confirmed by the application's business logic.
3. **Business-key relationships:** Relationships established through codes or identifiers even when an actual SQL Server foreign key constraint has not been confirmed.
4. **Unverified assumptions:** Relationships or meanings that have not been established. Do not treat these as confirmed facts.

Do not invent missing columns, relationships, operation meanings, or business rules.

---

## 2. Main Database Entities

The following tables are part of the documented Silo WMS database model.

| Table | Business Meaning |
|---|---|
| `tbl_Tags` | Registered inventory items and RFID-tagged products currently represented in the inventory table |
| `tbl_Products` | Master product definitions and technical information |
| `tbl_TagsMovement` | Item-level records associated with entry and exit/return movement histories |
| `tbl_MovementActions` | Operation or movement header records |
| `tbl_ActionTypes` | Operation types and their associated business configuration |
| `tbl_Destination` | Physical warehouses, storage destinations, and destination hierarchy |
| `tbl_DestinationType` | Categories or types of physical destinations |
| `tbl_Zones` | Storage locations or zones within warehouses |
| `tbl_ProductBrand` | Product brands |
| `tbl_ProductGroup` | Product groups |
| `tbl_ProductSubGroup` | Product subgroups |
| `tbl_ProductClass` | Product classes |
| `tbl_ProductStatus` | Product quality/status definitions |
| `tbl_ProductPropertyA` | Product property dimension A; used for concepts such as production line where applicable |
| `tbl_ProductPropertyB` | Product property dimension B; used for concepts such as work shift where applicable |
| `tbl_ProductPropertyC` | Product property dimension C; used for concepts such as size where applicable |
| `tbl_ProductType` | Product types or categories |
| `tbl_Station` | RFID gates or operational stations |
| `tbl_User` | System users and operators |
| `tbl_UHF_ReaderLog` | RFID reader detection logs |
| `tbl_TruckCross` | Truck or vehicle crossing records |
| `tbl_InventoryTags` | Records associated with handheld inventory counting (انبارگردانی دستی) |
| `tbl_ProductPlacementMissions` | Product/tag placement missions and movements between storage zones |

Use only tables whose existence and required columns have been confirmed in the actual database. A table's presence in this conceptual list does not establish every column or relationship attributed to it elsewhere.

---

## 3. Core Entity Definitions

### 3.1 `tbl_Tags` — Registered Inventory Items

This table represents registered inventory items and their associated RFID tags.

Important columns include:

| Column | Meaning |
|---|---|
| `Id` | Identity-generated identifier. It is not the business key used to relate inventory records to other tables. |
| `ProductSerial` | Product/item serial number |
| `TagEpc` | RFID tag EPC |
| `ProductCode` | Product code |
| `ProductName` | Product name stored on the tag record |
| `ProductType` | Product type code or stored type value; use the confirmed product-type mapping when resolving its title |
| `ProductStatus` | Stored product quality/status value |
| `ProductCount` | Quantity associated with the record |
| `TagStatus` | Tag lifecycle/status value |
| `TagInDestinationId` | Warehouse/destination code |
| `TagZone` | Storage zone code |
| `TagRegisterDateTime` | Gregorian registration datetime |
| `TagRegisterShamsiUnixDate` | Shamsi date/time represented as a string |
| `TagRegisterUser` | Registration user value |
| `Username` | User name stored on the record |
| `fld_LastModifierUser` | Last modifier user value |
| `fld_ProductPropertyAId` | Reference to product property A |
| `fld_ProductPropertyBId` | Reference to product property B |
| `fld_ProductPropertyCId` | Reference to product property C |
| `fld_ProductGroup` | Product group code |
| `fld_ProductBrand` | Product brand code |
| `fld_ProductSubGroup` | Product subgroup code |
| `fld_ProductClass` | Product class code |
| `ProductProperties` | Dynamic JSON data containing item-specific properties |
| `Lock` | Lock flag |
| `Freeze` | Freeze flag |
| `Deactivate` | Deactivation flag |
| `TagTreeParentId` | Parent identifier in the tag hierarchy |
| `TagTreeSecondParentId` | Secondary parent identifier in the tag hierarchy |
| `TagTreeParentsId` | Hierarchy-related parent data |
| `TagTreeParentSerial` | Parent serial number |
| `TagTreeParentsEpc` | Parent EPC-related data |

#### Primary Key

The confirmed primary key is the composite key:

`(ProductSerial, TagEpc)`

Although `Id` is an `IDENTITY` column and is populated automatically when a record is inserted, it is not the primary key used by the documented business relationships.

**Do not use `tbl_Tags.Id` as the relationship key for inventory movement history.** Use the documented serial/EPC relationship appropriate to the operation.

#### Inventory Status Flags

The flags `Lock`, `Freeze`, and `Deactivate` represent separate properties. Do not assume that one flag implies another.

Apply a `Deactivate` filter only when the user explicitly requests active or inactive items:

- If the user asks for **active items (فعال)**, filter using `Deactivate = 0`.
- If the user asks for **inactive items (غیرفعال)**, filter using `Deactivate = 1`.
- If the user does not mention active/inactive status, do not automatically filter on `Deactivate`.

Do not infer the meaning of `TagStatus`, `Lock`, or `Freeze` beyond the established application semantics.

#### Shamsi Registration Timestamp

`TagRegisterShamsiUnixDate` is an `nvarchar(50)` column. Its documented format is:

`YYYYMMDDHHmmss`

Example:

`14020618131759`

This represents a Shamsi year, month, day, hour, minute, and second. It is not a standard Gregorian `datetime` value.

Do not pass this field to `dbo.GeorgianDateToJalaliDate()`.

### 3.2 `tbl_Products` — Master Product Data

This table stores master product definitions and technical information.

Important columns:

- `ProductCode`
- `ProductTitle`
- `ProductTechnicalCode`
- `ProductSize`
- `ProductType`
- `ProductStatus`
- `fld_ProductGroup`
- `fld_ProductBrand`
- `fld_ProductSubGroup`
- `fld_ProductClass`
- `ProductProperties`
- `ProductTechnicalData`

`ProductCode` is the documented primary/business key for master product lookup. The provided table definition declares it `NOT NULL` but does not show an explicit primary key constraint. Confirm the actual database constraint before describing it as a physically enforced SQL Server primary key.

Use `tbl_Products` when the user asks about product definitions, technical codes, technical specifications, product properties, or master product information.

Use `ProductTechnicalData` for dynamic technical properties stored in JSON. Do not assume every technical property has a dedicated relational column.

### 3.3 `tbl_TagsMovement` — Item-Level Movement History

This table stores item-level movement records associated with operations.

Important columns:

- `TagsMovementId`
- `TagEpc`
- `ProductCode`
- `ProductSerial`
- `ProductCount`
- `RMovementActionId`
- `HMovementActionId`
- `RTagsMovementDate`
- `RTagsMovementTime`
- `RTagsMovementDateTime`
- `RTagsMovementSt`
- `HTagsMovementDate`
- `HTagsMovementTime`
- `HTagsMovementDateTime`
- `HTagsMovementSt`
- `MovementData`
- `RMovementActionType`
- `RMovementActionDocumentId`
- `RMovementActionUHFLogId`
- `RMovementActionDestinationId`

The two movement-action identifiers represent distinct business paths:

- `RMovementActionId`: entry/registration movement path.
- `HMovementActionId`: exit/return movement path.

They must be treated as two independent relationships. Do not substitute one for the other simply because both reference movement-action records.

`TagsMovementId` is an identity-generated identifier. The provided DDL does not declare it as a primary key, so verify the actual constraint before assuming that it is the enforced primary key.

### 3.4 `tbl_MovementActions` — Operation Header

This table represents operation or movement header records.

Important columns:

- `MovementActionId`
- `MovementActionTp`
- `MovementActionStore`
- `MovementActionDestinationId`
- `MovementActionUserId`
- `MovementActionDate`
- `MovementActionTime`
- `MovementActionDateTime`
- `MovementActionCountTags`
- `MovementActionData`
- `MovementActionUHFLogId`
- `MovementActionTruckCrossId`
- `MovementActionUHFLogGate`
- `MovementActionCarPlaque`
- `MovementActionDriverName`
- `MovementActionDriverMobile`
- `MovementActionDesc`
- `MovementActionDocumentId`
- `MovementActionLinkId`
- `MovementActionLinkDestId`

The provided DDL defines `MovementActionId` as nullable and does not declare a primary key constraint. Although this column is used as the movement reference in the documented business model, verify its actual uniqueness and constraints in SQL Server.

Do not assume a movement-action identifier is unique merely because the application uses it as an identifier.

### 3.5 `tbl_ActionTypes` — Operation Types

This table defines operation types and their business configuration.

Important columns:

- `Id`
- `fld_ActionTypeId`
- `fld_ActionTypeTitle`
- `fld_ActionTypeFromDestinationType`
- `fld_ActionTypeToTypeDestinationType`
- `fld_ActionTypePermitedDocStatus`
- `fld_ActionTypeChangeDocStatus`
- `fld_ActionTypeActiveControls`
- `fld_ActionTypeRfidPower`
- `fld_ActionTypeChangeTagLocation`
- `fld_ActionTypeProductType`

`Id` is an identity-generated identifier for the table record. The documented relationships with other tables use `fld_ActionTypeId`, not `Id`.

When resolving the type of an operation, use:

`tbl_MovementActions.MovementActionTp`  
→ `tbl_ActionTypes.fld_ActionTypeId`  
→ `tbl_ActionTypes.fld_ActionTypeTitle`

Do not replace `fld_ActionTypeId` with `Id` in a join unless the actual database relationship has been confirmed to use `Id`.

The supplied DDL does not declare a primary key or unique constraint on `fld_ActionTypeId`. Its business role as the operation-type reference is known, but its physical uniqueness constraint must be verified separately.

### 3.6 `tbl_Station` — RFID Gates and Operational Stations

Important columns:

- `fld_StationId`
- `fld_StationCode`
- `fld_StationName`
- `fld_StationType`
- `fld_StationActionType`
- `fld_StationStatus`
- `fld_StationReaders`
- `fld_StationDescription`
- `fld_StationSettings`
- `fld_StationFromDestination`
- `fld_StationToDestination`
- `fld_StationMacAddress`

The relationship between station and operation type is:

`tbl_Station.fld_StationActionType`  
→ `tbl_ActionTypes.fld_ActionTypeId`

Do not route this relationship through `tbl_TagsMovement`.

When resolving a station or gate recorded on an operation, use the separate relationship:

`tbl_MovementActions.MovementActionUHFLogGate`  
→ `tbl_Station.fld_StationCode`

These two relationships have different purposes: one associates a station with an operation type; the other identifies the station/gate associated with a movement action.

### 3.7 `tbl_Destination` — Physical Warehouses and Destinations

Important columns:

- `DestinationId`
- `DestinationCode`
- `DestinationTitle`
- `DestinationSt`
- `DestinationDesc`
- `DestinationType`
- `DestinationParentId`
- `DestinationParentsId`
- `DestinationEpc`

This table represents actual physical locations, warehouses, and destination hierarchy.

Distinguish carefully between:

- `DestinationId`: identity-generated row identifier.
- `DestinationCode`: business code used by the documented location relationships.
- `DestinationTitle`: display name.
- `DestinationType`: destination category/type.
- `DestinationParentId` and `DestinationParentsId`: hierarchy-related values.

Do not assume `DestinationId` and `DestinationCode` are interchangeable.

### 3.8 `tbl_DestinationType` — Warehouse/Destination Categories

Important columns:

- `fld_Id`
- `fld_DestinationTypeCode`
- `fld_DestinationTypeName`

This table represents the type or category of a physical destination, not a physical warehouse itself.

The conceptual relationship is:

`tbl_Destination.DestinationType`  
→ `tbl_DestinationType.fld_DestinationTypeCode`

The provided DDL does not declare a primary key or unique constraint on `fld_DestinationTypeCode`. Confirm actual constraints before assuming uniqueness.

### 3.9 `tbl_Zones` — Storage Zones

Important columns:

- `id`
- `ZoneCode`
- `ZoneTitle`
- `ZoneCapacity`
- `ZoneDimention`
- `ZoneParentCode`
- `ZoneParentLayer`
- `ZoneStoreCode`
- `ZoneCountPixle`
- `ZoneOccupiedCapacity`
- `MinZoneCapacity`
- `MaxZoneCapacity`
- `ZoneRowIndex`
- `ZoneType`
- `ZoneAddress`
- `ZoneCorridorId`
- `ZoneMapXPosition`
- `ZoneMapYPosition`

`ZoneCode` identifies a zone in the documented business relationship. `ZoneTitle` is the human-readable zone name. `ZoneStoreCode` stores the associated warehouse code.

The documented lookup paths are:

- Inventory zone: `tbl_Tags.TagZone` → `tbl_Zones.ZoneCode`
- Zone's warehouse: `tbl_Zones.ZoneStoreCode` → `tbl_Destination.DestinationCode`

The supplied DDL does not declare a primary key or unique constraint on `ZoneCode`. Verify uniqueness if a query relies on a one-to-one join.

### 3.10 Product Classification and Property Tables

The following tables contain product classification or property definitions:

- `tbl_ProductBrand`
- `tbl_ProductGroup`
- `tbl_ProductSubGroup`
- `tbl_ProductClass`
- `tbl_ProductStatus`
- `tbl_ProductPropertyA`
- `tbl_ProductPropertyB`
- `tbl_ProductPropertyC`
- `tbl_ProductType`

Use the code/reference fields for joins and the corresponding title/name fields for user-facing filtering and display, according to the mapping in Section 4.

Do not assume that identity columns are the business keys used by `tbl_Tags`.

#### Property A

`tbl_ProductPropertyA` includes:

- `fld_ProductPropertyAId`
- `fld_ProductPropertyATitle`
- `fld_ProductPropertyADesc`
- `fld_ProductPropertyAData`

The provided DDL defines `fld_ProductPropertyATitle` as unique. It does not declare `fld_ProductPropertyAId` as an identity column.

#### Property B

`tbl_ProductPropertyB` includes:

- `fld_ProductPropertyBId`
- `fld_ProductPropertyBTitle`
- `fld_ProductPropertyBDesc`
- `fld_ProductPropertyBData`
- `fld_ProductPropertyAId`

The `fld_ProductPropertyAId` column indicates a relationship to property A in the documented model. Verify the actual constraints and application behavior before assuming that the relationship is enforced by a foreign key.

Property B is also used by the documented shift-based business-date logic. See Section 9.

#### Property C

`tbl_ProductPropertyC` includes:

- `fld_ProductPropertyCId`
- `fld_ProductPropertyCTitle`
- `fld_ProductPropertyCDesc`
- `fld_ProductPropertyCData`
- `fld_ProductPropertyCIdentity`
- `fld_ProductPropertyCTemp`

`fld_ProductPropertyCIdentity` is an identity-generated column. It should not automatically be treated as the business key when relating property C to inventory records.

### 3.11 `tbl_User` — System Users

Important columns:

- `Id`
- `Name`
- `Username`
- `IsActive`
- `CreateDate`
- `LastModifiedDate`
- `CreatorIdentityID`
- `LastModifierIdentityID`

The supplied DDL declares `Id` as the primary key. It is an `nvarchar(128)` value, not an integer.

For operation-user lookup, the documented relationship is:

`tbl_MovementActions.MovementActionUserId`  
→ `tbl_User.Id`

The source definition gives `MovementActionUserId` as `nvarchar(max)`, so do not assume it has the same declared type or a physically enforced foreign key constraint.

Use `tbl_User.Name` for the operator's display name when this mapping applies. Use `Username` only when the request specifically requires the username or the application's business logic calls for it.

---

## 4. Lookup Mapping: Resolve User-Friendly Names Correctly

When a user asks for records by a name or title, use the relevant lookup table rather than comparing the user's text directly to a code or ID.

The following mapping describes the documented business relationships.

| User's Request | Reference Column | Lookup Table | Join Condition | Display/Filter Column |
|---|---|---|---|---|
| Warehouse / Destination (انبار) | `tbl_Tags.TagInDestinationId` | `tbl_Destination` | `TagInDestinationId = DestinationCode` | `DestinationTitle` |
| Zone / Location (ناحیه) | `tbl_Tags.TagZone` | `tbl_Zones` | `TagZone = ZoneCode` | `ZoneTitle` |
| Master Product (کالا) | `tbl_Tags.ProductCode` | `tbl_Products` | `ProductCode = ProductCode` | `ProductTitle` |
| Brand (برند) | `tbl_Tags.fld_ProductBrand` | `tbl_ProductBrand` | `fld_ProductBrand = fld_ProductBrandCode` | `fld_ProductBrandTitle` |
| Group (گروه کالا) | `tbl_Tags.fld_ProductGroup` | `tbl_ProductGroup` | `fld_ProductGroup = fld_ProductGroupCode` | `fld_ProductGroupTitle` |
| Subgroup (زیرگروه کالا) | `tbl_Tags.fld_ProductSubGroup` | `tbl_ProductSubGroup` | `fld_ProductSubGroup = fld_ProductSubGroupCode` | `fld_ProductSubGroupTitle` |
| Product Class (طبقه کالا) | `tbl_Tags.fld_ProductClass` | `tbl_ProductClass` | `fld_ProductClass = fld_ProductClassCode` | `fld_ProductClassTitle` |
| Property A / Production Line (خط تولید، where applicable) | `tbl_Tags.fld_ProductPropertyAId` | `tbl_ProductPropertyA` | `fld_ProductPropertyAId = fld_ProductPropertyAId` | `fld_ProductPropertyATitle` |
| Property B / Work Shift (شیفت کاری، where applicable) | `tbl_Tags.fld_ProductPropertyBId` | `tbl_ProductPropertyB` | `fld_ProductPropertyBId = fld_ProductPropertyBId` | `fld_ProductPropertyBTitle` |
| Property C / Size (سایز، where applicable) | `tbl_Tags.fld_ProductPropertyCId` | `tbl_ProductPropertyC` | `fld_ProductPropertyCId = fld_ProductPropertyCId` | `fld_ProductPropertyCTitle` |
| Quality Status (کیفیت کالا) | `tbl_Tags.ProductStatus` | `tbl_ProductStatus` | `ProductStatus = ProductStatusCode` | `ProductStatusTitle` |
| Product Type / Category (نوع کالا) | `tbl_Tags.ProductType` | `tbl_ProductType` | `ProductType = ProductTypeCode` | `ProductTypeTitle` |
| Operation Type (نوع عملیات) | `tbl_MovementActions.MovementActionTp` | `tbl_ActionTypes` | `MovementActionTp = fld_ActionTypeId` | `fld_ActionTypeTitle` |
| Station / Gate (گیت) | `tbl_MovementActions.MovementActionUHFLogGate` | `tbl_Station` | `MovementActionUHFLogGate = fld_StationCode` | `fld_StationName` |
| User / Operator (کاربر / اپراتور) | `tbl_MovementActions.MovementActionUserId` | `tbl_User` | `MovementActionUserId = Id` | `Name` |
| Warehouse Type (نوع انبار) | `tbl_Destination.DestinationType` | `tbl_DestinationType` | `DestinationType = fld_DestinationTypeCode` | `fld_DestinationTypeName` |

### 4.1 Filtering by Names

When the user specifies a Persian or English name, filter using the corresponding display/name column, usually with `LIKE` when partial matching is appropriate.

For example, a request for a warehouse by name should be resolved through `tbl_Destination.DestinationTitle`, not by comparing the Persian name directly against `tbl_Tags.TagInDestinationId`.

The same principle applies to product, zone, brand, group, subgroup, class, quality, product type, operation type, station, user, and warehouse type.

Use an exact comparison only when the user requests an exact match or the business requirement explicitly requires one.

### 4.2 Important Limitations

The mapping above is a business-key map. It does not prove that every relationship has a corresponding SQL Server foreign key constraint.

The supplied schema also contains type differences and columns whose uniqueness is not declared. Before assuming a join returns exactly one lookup record, verify the referenced code's uniqueness and the actual application behavior.

For example, the DDL for `tbl_ProductType` declares `ProductTypeCode` as nullable and does not declare it unique. A join through this column may therefore require further validation to prevent duplicate result rows.

---

## 5. Entity Resolution — Choosing the Correct Starting Table

Select the base table according to the user's intent. Do not begin every query from `tbl_Tags`.

### 5.1 Master Product Data (اطلاعات پایه کالا)

If the user asks about product definitions, technical codes, technical specifications, product properties, or master product information, start with:

`tbl_Products`

Use `tbl_Tags` only when the request also requires inventory/tag information.

### 5.2 Current Inventory and Tags (موجودی فعلی)

If the user asks about registered tags, current stock, current warehouse location, or the current state of tagged items, start with:

`tbl_Tags`

Use the appropriate lookup tables when the user asks for human-readable names.

### 5.3 Entry History (تاریخچه ورود)

If the user asks about goods entering the warehouse or entry history, start with:

`tbl_TagsMovement`

Relate the entry movement to `tbl_MovementActions` through:

`tbl_TagsMovement.RMovementActionId`  
→ `tbl_MovementActions.MovementActionId`

Use `ProductSerial` and, where needed, `TagEpc` to associate movement records with inventory/tag records.

### 5.4 Exit and Return History (تاریخچه خروج / برگشت)

If the user asks about goods leaving the warehouse or returning to the warehouse, start with:

`tbl_TagsMovement`

Relate the relevant history to `tbl_MovementActions` through:

`tbl_TagsMovement.HMovementActionId`  
→ `tbl_MovementActions.MovementActionId`

**Critical:** `HMovementActionId` alone does not prove that an operation represents a physical exit. Always verify the actual operation through:

- `tbl_MovementActions.MovementActionTp`
- `tbl_ActionTypes.fld_ActionTypeTitle`
- `tbl_MovementActions.MovementActionData`, when necessary

A return operation must not automatically be classified as an exit.

### 5.5 Operations and Movements (عملیات)

If the user asks about registered operations, operation dates, operators, gates, documents, operation types, or movement headers, start with:

`tbl_MovementActions`

Join `tbl_ActionTypes`, `tbl_User`, `tbl_Station`, `tbl_Destination`, or other related tables only when the question requires their information.

### 5.6 RFID Reader Logs (لاگ خوانش RFID)

If the user asks whether a tag was physically detected by an RFID reader, antenna, or gate, start with:

`tbl_UHF_ReaderLog`

This is distinct from movement history. A movement record does not, by itself, prove that a specific RFID read occurred.

The documented EPC relationship is:

`tbl_Tags.TagEpc` ↔ `tbl_UHF_ReaderLog.fld_TagSerial`

Verify the actual reader-log schema before using additional columns or assuming the relationship is enforced by a foreign key.

### 5.7 Truck and Vehicle Crossings (تردد کامیون / خودرو)

If the user asks about trucks, vehicle plates, drivers, shipments, or truck-crossing records, start with:

`tbl_TruckCross`

If the question concerns a movement operation associated with a truck crossing, use:

`tbl_MovementActions.MovementActionTruckCrossId`  
→ `tbl_TruckCross.fld_TruckCrossId`

The supplied definition uses `bigint` for `MovementActionTruckCrossId`; confirm the target column's type and relationship before relying on the join.

### 5.8 Handheld Inventory Counting (انبارگردانی دستی)

If the user asks about manual inventory counting performed using handheld devices (دستگاه هندهلد), start with:

`tbl_InventoryTags`

Do not automatically treat this as the same process as physical counting.

### 5.9 Physical Counting (شمارش فیزیکی)

Physical counting (شمارش فیزیکی) is a separate business concept from handheld inventory counting (انبارگردانی دستی).

It may involve other reader devices, physical-counting workflows, or Excel-based processes.

Do not automatically map a request about physical counting to `tbl_InventoryTags` unless the application's actual workflow establishes that relationship.

### 5.10 Placement Missions (ماموریت جانمایی)

If the user asks about product/tag placement or movement between storage zones, start with:

`tbl_ProductPlacementMissions`

The documented relationship is:

`tbl_ProductPlacementMissions.fld_PPMProductSerial`  
→ `tbl_Tags.ProductSerial`

Verify the actual mission schema and the business meaning of each status/date field before generating detailed placement reports.

---

## 6. Movement History and the Golden Relationship Chain

The main product-to-movement relationship is:

`tbl_Products`  
→ `ProductCode`  
→ `tbl_Tags`  
→ `ProductSerial`  
→ `tbl_TagsMovement`  
→ `RMovementActionId` or `HMovementActionId`  
→ `tbl_MovementActions`  
→ `MovementActionTp`  
→ `tbl_ActionTypes`

Each link has a different purpose:

1. `tbl_Products` describes the master product.
2. `tbl_Tags` represents registered inventory/tag records.
3. `tbl_TagsMovement` represents item-level movement history.
4. `tbl_MovementActions` represents the associated operation header.
5. `tbl_ActionTypes` identifies the configured operation type.

### 6.1 Entry and Exit/Return Are Separate Paths

For entry/registration history:

`tbl_TagsMovement.RMovementActionId`  
→ `tbl_MovementActions.MovementActionId`

For exit/return history:

`tbl_TagsMovement.HMovementActionId`  
→ `tbl_MovementActions.MovementActionId`

Do not combine these paths into a single join condition without a specific business requirement. Joining both paths indiscriminately can introduce duplicate rows or associate a line with the wrong operation.

### 6.2 Determine the Actual Operation Type

For an operation-related question, use:

`tbl_MovementActions.MovementActionTp`  
→ `tbl_ActionTypes.fld_ActionTypeId`

Then inspect `fld_ActionTypeTitle` to identify the configured operation type.

If the operation title is insufficient to establish its exact meaning, inspect `MovementActionData` and the relevant application business logic.

Do not infer a physical exit, entry, return, or transfer solely from the name of a reference column.

### 6.3 Product Serial and EPC

`ProductSerial` identifies the product/item serial in the documented movement relationship.

`TagEpc` identifies the RFID tag EPC.

The composite primary key of `tbl_Tags` is `(ProductSerial, TagEpc)`. `Id` is an identity-generated field and is not the business relationship key for the documented movement paths.

When linking movement lines to tags, select the appropriate business key for the question. If the request concerns a specific RFID tag, EPC may be necessary in addition to the serial number.

---

## 7. Location Logic — Destination vs. Destination Type

Never confuse `tbl_Destination` with `tbl_DestinationType`.

- `tbl_Destination`: An actual physical warehouse, storage location, or destination.
- `tbl_DestinationType`: A category/type assigned to a physical destination.

The conceptual relationship is:

`tbl_Destination.DestinationType`  
→ `tbl_DestinationType.fld_DestinationTypeCode`

### 7.1 Operation Location References

`tbl_MovementActions.MovementActionStore` references a `DestinationCode` in `tbl_Destination` according to the documented business model.

`tbl_MovementActions.MovementActionDestinationId` also references a `DestinationCode` in `tbl_Destination`.

These fields represent different location roles within an operation. Do not automatically assume that `MovementActionStore` always represents the physical source (مبدأ) in every business scenario.

Determine the actual source/target semantics from the operation type and its business logic.

### 7.2 Operation Type and Location Types

`tbl_ActionTypes` contains:

- `fld_ActionTypeFromDestinationType`: Source location type.
- `fld_ActionTypeToTypeDestinationType`: Target location type.

These values relate conceptually to `tbl_DestinationType`.

To understand an operation in terms of:

`Source Location Type (نوع مکان مبدأ)`  
→ `Operation Type (نوع عملیات)`  
→ `Target Location Type (نوع مکان مقصد)`

the query may need to combine:

- `tbl_ActionTypes`
- `tbl_DestinationType`
- `tbl_Destination`
- `tbl_MovementActions`

Use the actual operation type and the configured location-type values to interpret the operation. Do not infer the source or target solely from a column name.

---

## 8. Dynamic JSON Properties

Some business properties are stored in JSON columns rather than in ordinary relational columns.

Use SQL Server's `JSON_VALUE` to extract a scalar property from a JSON document.

### 8.1 Tag or Item Properties

Source:

`tbl_Tags.ProductProperties`

Example:

`JSON_VALUE(t.ProductProperties, N'$."FieldName"')`

Use this when the requested property belongs to the registered tag/item record.

### 8.2 Operation Properties

Source:

`tbl_MovementActions.MovementActionData`

Example:

`JSON_VALUE(ma.MovementActionData, N'$."FieldName"')`

Use this when the requested property belongs to the operation or movement header.

### 8.3 Master Product Technical Properties

Source:

`tbl_Products.ProductTechnicalData`

Example:

`JSON_VALUE(p.ProductTechnicalData, N'$."FieldName"')`

Use this when the requested property belongs to the master product's technical data.

### 8.4 JSON Interpretation Rules

Before writing a query:

1. Determine which entity owns the requested property.
2. Identify the correct JSON column.
3. Determine the actual JSON property name and path from the data or application definition.
4. Use `JSON_VALUE` for scalar properties.
5. Do not assume that a JSON property exists in every record.
6. Do not invent a JSON property name or treat an unverified property as a confirmed schema field.

A property with a similar name may exist in multiple JSON columns. The correct source depends on the business meaning of the request.

---

## 9. Aggregation Rules: Unique Items vs. Total Quantity

Never confuse the number of unique product items with the sum of their quantities.

### 9.1 Count Unique Serials or Items

If the user asks:

- How many unique items or serials exist?
- تعداد سریال
- تعداد کالا, when the intended meaning is the number of distinct product serials
- چند تگ داریم؟, when the intended meaning is distinct item serials

Use a distinct serial count:

`COUNT(DISTINCT ProductSerial)`

For movement history, this may be:

`COUNT(DISTINCT tm.ProductSerial)`

Use the correct alias and base table for the query.

### 9.2 Sum Total Quantity

If the user asks:

- What is the total quantity?
- مجموع موجودی
- مجموع تعداد کالا
- چه مقدار کالا داریم؟

Use:

`SUM(ProductCount)`

For current inventory, this may be:

`SUM(t.ProductCount)`

For movement history, use the quantity column from the appropriate movement records if the question concerns quantities moved.

### 9.3 Do Not Mix the Two Metrics

`COUNT(DISTINCT ProductSerial)` counts distinct serials.

`SUM(ProductCount)` adds the stored quantity values.

These measures are not interchangeable. The wording of the request and the selected base table must determine which metric is appropriate.

### 9.4 Avoid Duplicate-Row Inflation

When joining movement records to lookup tables or other one-to-many relationships, validate the join cardinality before aggregating.

An incorrect join can multiply rows and inflate a `SUM`. Use the correct relationship path and aggregate at the intended business level.

Do not use `DISTINCT` as a substitute for understanding the relationship structure.

---

## 10. Date and Time Logic

Date filtering and date display are separate operations. They must not be confused.

### 10.1 Date Fields Have Different Storage Formats

The database contains different date representations, including:

- Gregorian `datetime` columns, such as `TagRegisterDateTime` and `MovementActionDateTime`.
- Separate date and time fields, such as `MovementActionDate` and `MovementActionTime`.
- Shamsi date strings, such as `TagRegisterShamsiUnixDate`.
- Movement-specific date/time fields, such as `RTagsMovementDateTime` and `HTagsMovementDateTime`.

Do not assume all date fields use the same storage format or business meaning.

Before writing a date filter, identify the actual field and its storage format.

### 10.2 Convert User-Provided Shamsi Dates for Filtering

When a user provides a Persian/Shamsi date that must be compared against a Gregorian database date, use:

`dbo.JalaliDateToGeorgianDate`

This function is for converting the input Shamsi date to Gregorian format for filtering.

Do not use `dbo.GeorgianDateToJalaliDate()` in the `WHERE` clause as a substitute for correct date filtering.

Keep the filter aligned with the stored data type and the intended date range.

### 10.3 Display Gregorian Dates as Shamsi

Whenever a Gregorian date or datetime value is included in the final `SELECT` result for display to the user, convert it to Shamsi/Jalali format using:

`dbo.GeorgianDateToJalaliDate()`

This rule applies to Gregorian date/datetime fields across the database, including:

- Operation dates
- Product registration dates
- Tag registration dates
- Entry dates
- Exit dates
- Return dates
- Placement dates
- Inventory dates
- RFID log dates
- Other Gregorian date/datetime fields

Do not display a raw Gregorian date directly in the final report when the value is intended for user-facing display.

### 10.4 Separate Date and Time Fields

If date and time are stored separately:

- Convert the Gregorian date field to Shamsi for display.
- Keep the time field unchanged.

If date and time are stored together in a single datetime field:

- Display the date portion in Shamsi/Jalali format.
- Preserve the time portion.

Use a conversion expression compatible with the actual signature and return type of `dbo.GeorgianDateToJalaliDate()`.

### 10.5 Do Not Convert Dates That Are Already Shamsi

If a field is already stored as a Shamsi date or a Shamsi timestamp string, do not apply `dbo.GeorgianDateToJalaliDate()` to it.

In particular, do not treat `tbl_Tags.TagRegisterShamsiUnixDate` as a Gregorian datetime.

### 10.6 Shift-Based Business Days

The Silo WMS uses shift-based business logic. A business day must not automatically be assumed to start at `00:00`.

When the question depends on a shift-based date boundary, consult the shift definitions in:

`tbl_ProductPropertyB`

Relevant fields include:

- `fld_ProductPropertyBTitle`
- `fld_ProductPropertyBDesc`

Shift definitions may use a format such as:

`HH:mm-HH:mm`

The shift start time must be considered when determining the actual beginning and end of a business day.

Do not assume the description always follows the same format without checking the stored data. If the requested boundary cannot be determined from the documented schema and data, do not invent it.

---

## 11. Core Relationship Map

### 11.1 Product and Inventory

`tbl_Products.ProductCode`  
↔ `tbl_Tags.ProductCode`

This relates a master product definition to its registered inventory/tag records.

### 11.2 Inventory and Movement History

`tbl_Tags.ProductSerial`  
↔ `tbl_TagsMovement.ProductSerial`

The appropriate movement-action path is then:

- Entry/registration: `RMovementActionId`
- Exit/return: `HMovementActionId`

Both paths lead to `tbl_MovementActions.MovementActionId`.

### 11.3 Operation Type

`tbl_MovementActions.MovementActionTp`  
→ `tbl_ActionTypes.fld_ActionTypeId`

Use `fld_ActionTypeTitle` to display the operation type.

### 11.4 Operation Locations

`tbl_MovementActions.MovementActionStore`  
→ `tbl_Destination.DestinationCode`

`tbl_MovementActions.MovementActionDestinationId`  
→ `tbl_Destination.DestinationCode`

`tbl_Destination.DestinationType`  
→ `tbl_DestinationType.fld_DestinationTypeCode`

### 11.5 Inventory Location

`tbl_Tags.TagInDestinationId`  
→ `tbl_Destination.DestinationCode`

`tbl_Tags.TagZone`  
→ `tbl_Zones.ZoneCode`

`tbl_Zones.ZoneStoreCode`  
→ `tbl_Destination.DestinationCode`

### 11.6 Product Classification

The following code-based relationships are documented:

- `tbl_Tags.fld_ProductBrand` → `tbl_ProductBrand.fld_ProductBrandCode`
- `tbl_Tags.fld_ProductGroup` → `tbl_ProductGroup.fld_ProductGroupCode`
- `tbl_Tags.fld_ProductSubGroup` → `tbl_ProductSubGroup.fld_ProductSubGroupCode`
- `tbl_Tags.fld_ProductClass` → `tbl_ProductClass.fld_ProductClassCode`
- `tbl_Tags.ProductStatus` → `tbl_ProductStatus.ProductStatusCode`
- `tbl_Tags.ProductType` → `tbl_ProductType.ProductTypeCode`

### 11.7 Product Properties

- `tbl_Tags.fld_ProductPropertyAId` → `tbl_ProductPropertyA.fld_ProductPropertyAId`
- `tbl_Tags.fld_ProductPropertyBId` → `tbl_ProductPropertyB.fld_ProductPropertyBId`
- `tbl_Tags.fld_ProductPropertyCId` → `tbl_ProductPropertyC.fld_ProductPropertyCId`

### 11.8 Station and User

`tbl_MovementActions.MovementActionUHFLogGate`  
→ `tbl_Station.fld_StationCode`

`tbl_MovementActions.MovementActionUserId`  
→ `tbl_User.Id`

The station's operation-type configuration is a separate relationship:

`tbl_Station.fld_StationActionType`  
→ `tbl_ActionTypes.fld_ActionTypeId`

### 11.9 RFID Logs

`tbl_Tags.TagEpc`  
↔ `tbl_UHF_ReaderLog.fld_TagSerial`

Use this relationship for RFID detection questions, subject to validation of the actual reader-log schema.

### 11.10 Truck Crossings

`tbl_MovementActions.MovementActionTruckCrossId`  
→ `tbl_TruckCross.fld_TruckCrossId`

### 11.11 Placement Missions

`tbl_ProductPlacementMissions.fld_PPMProductSerial`  
→ `tbl_Tags.ProductSerial`

### 11.12 Handheld Inventory

`tbl_InventoryTags.fld_InventoryTagEPC`  
→ `tbl_Tags.TagEpc`

Use this relationship for handheld inventory records, not as an automatic substitute for physical-counting workflows.

---

## 12. SQL Server Schema Facts and Important Constraints

The supplied DDL establishes several important facts that must be respected.

### 12.1 `tbl_Tags`

- `Id` is `int IDENTITY(1,1) NOT NULL`.
- The primary key is `(ProductSerial, TagEpc)`.
- `ProductSerial` and `TagEpc` are both `NOT NULL`.
- `ProductCount` is `decimal(18,2)`.
- `TagRegisterDateTime` is `datetime`.
- `TagRegisterShamsiUnixDate` is `nvarchar(50)`.
- `ProductProperties` is `nvarchar(max)`.

### 12.2 `tbl_ActionTypes`

- `Id` is `int IDENTITY(1,1) NOT NULL`.
- `fld_ActionTypeId` is `int NULL`.
- The documented business relationships use `fld_ActionTypeId`.
- The supplied DDL does not declare a primary key or unique constraint on `fld_ActionTypeId`.

### 12.3 `tbl_Station`

- `fld_StationId` is `int IDENTITY(1,1) NOT NULL`.
- `fld_StationCode` is `nvarchar(128) NULL`.
- `fld_StationActionType` is `int NULL`.
- The documented station-to-operation-type relationship is through `fld_StationActionType`.

### 12.4 `tbl_MovementActions`

- `MovementActionId` is `int NULL` in the supplied DDL.
- The supplied DDL does not declare a primary key on this column.
- `MovementActionDateTime` is `datetime NULL`.
- `MovementActionDate` and `MovementActionTime` are separate string columns.
- `MovementActionData` is `nvarchar(max)`.
- `MovementActionTruckCrossId` is `bigint NULL`.

Do not silently assume a primary key, uniqueness constraint, or non-null constraint that is absent from the provided DDL.

### 12.5 `tbl_TagsMovement`

- `TagsMovementId` is `int IDENTITY(1,1) NOT NULL`.
- `RMovementActionId` and `HMovementActionId` are separate nullable integer columns.
- Entry and exit/return date/time values are stored in separate R and H fields.
- `ProductCount` is `decimal(18,2) NULL`.
- `MovementData` is `nvarchar(max)`.
- The supplied DDL does not declare a primary key or foreign key constraints.

### 12.6 Lookup Code Uniqueness

The provided DDL does not establish uniqueness for every business code used by the documented joins. Examples include:

- `DestinationCode`
- `ZoneCode`
- `fld_ActionTypeId`
- `fld_DestinationTypeCode`
- `ProductTypeCode`
- Various product classification codes

Before assuming a lookup join is one-to-one, verify the actual database constraints and the data's uniqueness.

A documented business relationship does not automatically imply an enforced SQL Server foreign key.

---

## 13. Query Generation Workflow

Before generating any SQL query, follow this sequence.

1. **Identify the user's intent.** Determine whether the question concerns master products, current inventory, entry, exit, return, movements, RFID reads, placement, inventory counting, or truck crossings.

2. **Select the correct base table.** Use the entity-resolution rules in Section 5.

3. **Identify the business key.** Use the documented serial, EPC, code, or identifier appropriate to the relationship. Do not substitute identity-generated row IDs for business keys without confirmation.

4. **Follow the correct relationship path.** Use the relationship map in Section 11.

5. **Verify operation meaning.** For movement-related requests, resolve `MovementActionTp` through `tbl_ActionTypes.fld_ActionTypeId` and inspect the operation title. Consult `MovementActionData` when necessary.

6. **Resolve user-facing names through lookup tables.** Use the corresponding title/name column instead of comparing a Persian name directly to a code or ID.

7. **Distinguish physical destinations from destination types.** Use `tbl_Destination` for actual locations and `tbl_DestinationType` for their categories.

8. **Resolve dynamic properties correctly.** Use `JSON_VALUE` against the JSON column belonging to the relevant entity.

9. **Select the correct aggregation.** Use `COUNT(DISTINCT ProductSerial)` for unique serial counts and `SUM(ProductCount)` for total quantity, according to the question.

10. **Respect date storage and business boundaries.** Convert user-provided Shamsi dates for filtering using `dbo.JalaliDateToGeorgianDate()`. Respect shift-based business-day boundaries when required.

11. **Format dates for display.** Convert Gregorian date/datetime values in the final result using `dbo.GeorgianDateToJalaliDate()`. Do not convert fields that are already stored as Shamsi dates.

12. **Check join cardinality.** Ensure joins do not introduce duplicate records that distort counts or sums.

13. **Avoid unsupported assumptions.** Do not invent table relationships, columns, JSON paths, operation meanings, or business rules.

14. **Validate the query's business meaning.** A syntactically valid SQL statement is not sufficient. Its joins, filters, aggregations, and date boundaries must reflect the actual Silo WMS business logic.

---

## 14. Final Principles

The following principles are mandatory when interpreting the Silo WMS database:

- `tbl_Tags.Id` is identity-generated; the documented composite primary key is `(ProductSerial, TagEpc)`.
- `tbl_ActionTypes.Id` is identity-generated; documented operation-type references use `fld_ActionTypeId`.
- `tbl_Station.fld_StationActionType` relates to `tbl_ActionTypes.fld_ActionTypeId`.
- `RMovementActionId` and `HMovementActionId` are distinct movement-history paths.
- `HMovementActionId` alone does not prove that an operation is a physical exit.
- `tbl_Destination` represents physical locations; `tbl_DestinationType` represents destination categories.
- Current inventory, movement history, RFID reads, handheld inventory counting, physical counting, and placement missions are distinct business concepts.
- Filter by display/name columns when the user supplies a name, using the correct lookup relationship.
- Use `JSON_VALUE` for properties stored in JSON columns.
- Distinguish unique serial counts from summed quantities.
- Respect the actual storage format of every date field and apply shift-based boundaries when required.
- Display Gregorian dates as Shamsi using `dbo.GeorgianDateToJalaliDate()`, and convert user-provided Shamsi dates for filtering using `dbo.JalaliDateToGeorgianDate()`.
- Do not assume relationships are physically enforced by foreign keys unless confirmed by the SQL Server schema.
- Do not invent unsupported schema facts or business behavior.

**The objective is not merely to generate syntactically valid SQL. The objective is to generate SQL that accurately represents the Silo WMS database and its real business processes.**