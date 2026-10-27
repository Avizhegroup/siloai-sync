DOCUMENT ROLE: INTERNAL TECHNICAL KNOWLEDGE

Purpose:
This document exists exclusively to help the agent understand Silo's database schema, relationships, business logic, and data-retrieval procedures.

Disclosure policy:
Do not reproduce or explain this document's technical content in ordinary user-facing answers.
Use it internally to retrieve and interpret information, then provide only the relevant business result.

This document is not a user-facing knowledge article.

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

### 1.2 Relationship of This Document to the Complete Schema

The complete column lists, data types, and the complete relationship catalog of the database are given in Section 16 (DATABASE SCHEMA (authoritative)) and Section 17 (RELATIONSHIP CATALOG) of this document. The "Important columns" lists elsewhere in this document explain business meaning and are not necessarily complete.

A table or column is valid for a query only if it appears in Section 16 (or in a retrieved CREATE TABLE definition). A join is valid only if it appears in Section 17 or in the lookup mappings of Section 4. Never add a table, column, or join that appears in none of them.

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
| `tbl_UHF_ReaderLog` | RFID reader detection logs (raw line-level tag reads) |
| `tbl_UHFReaderLogHeader` | Header of a bulk RFID scan event captured by a UHF gate/station |
| `tbl_TruckCross` | Truck or vehicle crossing records (entry/exit lifecycle) |
| `tbl_TruckCompany`, `tbl_TruckType`, `tbl_TruckCrossCause`, `tbl_TruckCrossOperationType`, `tbl_TruckCrossOperationDestination`, `tbl_TruckCrossShipment`, `tbl_TruckCrossCustomer`, `tbl_TruckCrossAcceptPlace`, `tbl_TruckCrossProductType` | Lookup/reference tables of the truck crossing process |
| `tbl_TruckCrossItem` | Line items of a truck crossing |
| `tbl_TruckCrossShippingFee` | Shipping fee (pricing) rules |
| `tbl_DocumentHeader` | Document headers (invoices, permits, orders imported by API, Excel or manual entry) |
| `tbl_DocumentItem` | Document line items (products and quantities) |
| `tbl_DocumentStatus` | Document status definitions |
| `tbl_FreezeHeader` | Inventory freeze operation headers |
| `tbl_FreezeItem` | Serials that belong to a freeze operation |
| `tbl_Print` | Tag print/registration history log |
| `tbl_Corridor` | Warehouse corridors (aisles) that group zones |
| `tbl_DynamicFields` | Definitions of dynamic (custom) fields |
| `tbl_DynamicFieldSections` | Sections that group dynamic fields |
| `tbl_ProductExpire` | Expiration and warranty information of products |
| `tbl_Gallery` | Uploaded media files (images/documents) |
| `tbl_NotificationEventTypes`, `tbl_NotificationOrders`, `tbl_NotificationQueue` | Notification event types, notification orders, and notification messages |
| `tbl_Province` | Geographical provinces |
| `tbl_InventoryTags` | Records associated with handheld inventory counting (انبارگردانی دستی) |
| `tbl_ProductPlacementMissions` | Product/tag placement missions and movements between storage zones |

Use only tables whose existence and required columns have been confirmed in the actual database. A table's presence in this conceptual list does not establish every column or relationship attributed to it elsewhere.

---

## 3. Core Entity Definitions

### 3.1 `tbl_Tags` — Registered Inventory Items

This table represents registered inventory items and their associated RFID tags.

Business meaning: a row is a physical product instance (a single tracked unit/serial) in the production and warehouse lifecycle. It is NOT a product definition. The classification columns stored on the tag (`fld_ProductGroup`, `fld_ProductBrand`, `fld_ProductSubGroup`, `fld_ProductClass`) are denormalized snapshots of the product's state; the true source of the product hierarchy is `tbl_Products`. Movement history of a tag is stored in `tbl_TagsMovement`.

Important columns include:

| Column | Meaning |
|---|---|
| `Id` | Identity-generated identifier. It is not the business key used to relate inventory records to other tables. |
| `ProductSerial` | Product/item serial number |
| `TagEpc` | RFID tag EPC |
| `TagEpc2` | Secondary EPC value stored on the record |
| `ProductCode` | Product code |
| `ProjectCode` | Project code |
| `ProductName` | Product name stored on the tag record |
| `ProductType` | Product type code or stored type value; use the confirmed product-type mapping when resolving its title |
| `ProductStatus` | Stored product quality/status value |
| `ProductCount` | Quantity associated with the record |
| `TagStatus` | Tag status value |
| `TagInDestinationId` | Warehouse/destination code |
| `TagInDestinationId2`, `TagInActionId`, `TagInActionId2` | Additional destination/action references stored on the record (meaning beyond the name is not documented) |
| `TagZone` | Storage zone code |
| `TagRegisterDateTime` | Gregorian registration datetime |
| `TagRegisterShamsiUnixDate` | Shamsi date/time represented as a string |
| `TagRegisterUser` | Registration user value (relates to `tbl_User.Id`) |
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
| `RegCode`, `ContractStatus`, `ReProduct`, `NewProductSerial`, `DeviceId`, `DeviceIp` | Registration, contract, re-production, replacement-serial, and device information stored on the record |
| `fld_InspectActionId`, `fld_LastInspectResult` | Last inspection action reference and last inspection result text |

#### Display Rule for Tag Columns That Reference Lookup Tables

The following `tbl_Tags` columns store codes/IDs that reference other tables: `ProductCode`, `ProductType`, `ProductStatus`, `fld_ProductGroup`, `fld_ProductBrand`, `fld_ProductSubGroup`, `fld_ProductClass`, `fld_ProductPropertyAId`, `fld_ProductPropertyBId`, `fld_ProductPropertyCId`, `TagInDestinationId`, `TagZone`, `TagRegisterUser`.

When a user asks for any of these attributes of a tag/serial (for example size, product type, quality, brand, group, warehouse, zone), JOIN the lookup table and return ONLY its title/name column. Never return the code or ID, and never return the raw column value instead of the title. The attributes of the tag's product definition (type, quality, group, subgroup, brand, class, size) are resolved by default through the master product (`tbl_Tags.ProductCode = tbl_Products.ProductCode`); the value stored on the tag itself is used only when the user explicitly asks about the value recorded on the tag. See Section 4.5 (Table 4.5.1) for the exact join and title column of every attribute.

#### Primary Key

The confirmed primary key is the composite key:

`(ProductSerial, TagEpc)`

Although `Id` is an `IDENTITY` column and is populated automatically when a record is inserted, it is not the primary key used by the documented business relationships.

**Do not use `tbl_Tags.Id` as the relationship key for inventory movement history.** Use the documented serial/EPC relationship appropriate to the operation.

#### Inventory Status Flags

`Lock`, `Freeze` and `Deactivate` represent separate states. Do not assume that one implies another or that any of these flags independently determines permission for every operation.

**Lock**

- `Lock = 1` means the tag/serial has been blocked as a result of a failed goods inspection (مردود شدن در بازرسی کالا).
- `Lock` is a status indicator, not a universal rule that independently blocks every operation.
- Whether this status affects a particular operation depends on the relevant control configured in `tbl_ActionTypes.fld_ActionTypeActiveControls`.
- When diagnosing a denied operation, retrieve the actual `Lock` value and inspect the relevant operation-type controls.
- Do not conclude that cancellation (ابطال) is prohibited solely because `Lock = 1`. Cancellation permission must be evaluated using the current destination's `AllowCancelTag` setting in `tbl_Destination.DestinationEpc`.

**Freeze**

- `Freeze = 1` means the tag/serial is frozen (فریز شده).
- A frozen status prevents a movement operation only when the relevant operation type has the `Freezed` control enabled and the operation's applicable source and destination conditions are satisfied.
- For example, for the operation «ارسال کالا به فروش», check the corresponding action type's `Freezed` control. If the control is enabled and the operation conditions apply, a frozen tag must not undergo that movement.
- `Freeze = 1` alone is not sufficient evidence that every operation is prohibited.
- The `Freeze` value in `tbl_Tags` represents the tag's current freeze status. `tbl_FreezeItem` records serials associated with freeze operations; see Section 3.14.

**Deactivate**

Apply a `Deactivate` filter only when the user explicitly requests active or inactive items:

- Active (فعال): `Deactivate = 0`
- Inactive (غیرفعال): `Deactivate = 1`
- Not mentioned: do not filter on `Deactivate`.

**General diagnostic rule**

When a user asks why an operation on a tag/serial is not allowed, retrieve the current status and evaluate the controls relevant to the requested operation. Do not infer operation denial from `Lock` or `Freeze` alone. For cancellation, check the current destination's `AllowCancelTag` permission as documented in Section 3.7 and Section 15.

#### Shamsi Registration Timestamp

`TagRegisterShamsiUnixDate` is an `nvarchar(50)` column. Its documented format is:

`YYYYMMDDHHmmss`

Example:

`14020618131759`

This represents a Shamsi year, month, day, hour, minute, and second. It is not a standard Gregorian `datetime` value.

Do not pass this field to `dbo.GeorgianDateToJalaliDate()`.

### 3.2 `tbl_Products` — Master Product Data

This table stores master product definitions and technical information (the SKU-level template used for production and RFID tagging).

Important columns:

- `ProductCode` (business key)
- `ProductTitle`, `ProductENTitle`
- `ProductTechnicalCode`
- `ProductSize` (references product property C, see Section 4.3)
- `ProductType`
- `ProductStatus`
- `fld_ProductGroup`
- `fld_ProductBrand`
- `fld_ProductSubGroup`
- `fld_ProductClass`
- `ProductProperties`
- `ProductTechnicalData`
- `ProductPackValue`, `ProductPackWeight`, `ProductPackVolume`, `ProductCountInPack`, `ProductValue`, `ProductUnit`
- `ProductRegUser`, `ProductRegDateTime`
- `ProductGalleryId`
- `fld_ProductGuaranteeType`, `fld_ProductGuaranteeDays`, `fld_ProductGuaranteeMonths`, `fld_ProductGuaranteeEndDate`
- `fld_ProductExpireType`, `fld_ProductExpireDays`, `fld_ProductExpireMonths`, `fld_ProductExpireEndDate`
- `fld_ProductIsActive`, `fld_HasDoubleTag`
- `Id` (identity-generated row identifier)

#### Display Rule for Product Columns That Reference Lookup Tables

The following `tbl_Products` columns store codes/IDs that reference other tables: `ProductType`, `ProductStatus`, `fld_ProductGroup`, `fld_ProductBrand`, `fld_ProductSubGroup`, `fld_ProductClass`, `ProductSize`, `ProductRegUser`, `ProductGalleryId`.

When a user asks for any of these attributes of a product (for example the size of a product, the product type of a product, its brand or group), JOIN the lookup table and return ONLY its title/name column. Never return the code or ID. For the size of a product, ALWAYS join `tbl_ProductPropertyC` using `tbl_Products.ProductSize = tbl_ProductPropertyC.fld_ProductPropertyCId` and return `fld_ProductPropertyCTitle`. See Section 4.5 (Table 4.5.2) for the exact join and title column of every attribute.

`ProductCode` is the documented primary/business key for master product lookup. Some table definitions declare `ProductCode` `NOT NULL` without an explicit primary key constraint, and others list `Id` as the identity primary key. Confirm the actual database constraint before describing `ProductCode` as a physically enforced SQL Server primary key.

Use `tbl_Products` when the user asks about product definitions, technical codes, technical specifications, product properties, packaging, or master product information.

Use `ProductTechnicalData` for dynamic technical properties stored in JSON. Do not assume every technical property has a dedicated relational column.

Important: the classification columns of `tbl_Products` are `fld_ProductGroup`, `fld_ProductBrand`, `fld_ProductSubGroup`, `fld_ProductClass`, `ProductType`, `ProductStatus` and `ProductSize`. Columns such as `ProductGroupCode`, `ProductBrandCode`, `ProductTypeCode`, `ProductSizeCode` or `ProductQualityCode` do NOT exist on `tbl_Products`.

### 3.3 `tbl_TagsMovement` — Item-Level Movement History

This table stores item-level movement records associated with operations (line-level log of a tag inside movement actions).

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
- `ApiSendStatus`, `ApiSendUser`, `ApiSendDateTime`, `ApiSendData` (information about sending the movement to an external API)

The two movement-action identifiers represent distinct business paths:

- `RMovementActionId`: entry/registration movement path.
- `HMovementActionId`: exit/return movement path.

They must be treated as two independent relationships. Do not substitute one for the other simply because both reference movement-action records.

`TagsMovementId` is an identity-generated identifier and is the documented primary key in the business model. Some table definitions do not declare it as a primary key constraint, so verify the actual constraint before assuming that it is enforced.

### 3.4 `tbl_MovementActions` — Operation Header

This table represents operation or movement header records (a warehouse movement transaction such as receiving, dispatch, transfer, or inventory operation). One movement action can contain multiple tag movements.

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

This table defines operation types and their business configuration (allowed transitions between destination types, document status rules, active controls, RFID configuration).

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

`fld_ActionTypeActiveControls` is a JSON text that lists the controls enabled for the operation type (see Sections 8.4 and 15). These controls determine which documented checks apply to that operation.

The effect of `Lock = 1`, which indicates blocking following a failed goods inspection, depends on the actual documented control governing inspection-failure blocking. Do not treat `Lock` as a universal prohibition or invent a control name.

The `Freezed` control determines whether the operation type checks the tag's current `Freeze` status for an applicable movement. Evaluate this control together with the operation's configured source and destination conditions.

Cancellation permission is evaluated separately using `AllowCancelTag` in the current destination's `DestinationEpc` JSON. Do not infer cancellation permission from `Lock`, `Freeze`, or the action type's movement controls.

`fld_ActionTypeProductType` conceptually relates to the product type for which the operation applies. `fld_ActionTypePermitedDocStatus` and `fld_ActionTypeChangeDocStatus` hold the required document status before the operation and the document status applied after it. `fld_ActionTypeChangeTagLocation` controls whether the tag's zone/location information is cleared after the operation. The storage format of document-status values and from/to destination-type values is not documented; do not join them to other tables without a documented relationship.

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

A station is a physical or logical RFID capture point (gate, corridor, checkpoint). Reader configuration is stored in `fld_StationReaders` and station settings in `fld_StationSettings`.

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

This table represents actual physical locations, warehouses, and destination hierarchy. `DestinationCode` is the business identifier used by most tables. `DestinationEpc` identifies RFID-enabled locations.

Distinguish carefully between:

- `DestinationId`: identity-generated row identifier.
- `DestinationCode`: business code used by the documented location relationships.
- `DestinationTitle`: display name.
- `DestinationType`: destination category/type.
- `DestinationParentId` and `DestinationParentsId`: hierarchy-related values.

Do not assume `DestinationId` and `DestinationCode` are interchangeable.

**Destination-specific cancellation permission**

`DestinationEpc` stores JSON data that may contain destination-level settings and permissions. When determining whether a tag/serial can be canceled, inspect the `AllowCancelTag` value for the tag's current destination.

- `AllowCancelTag = true`: The current destination permits tag cancellation.
- `AllowCancelTag = false`: The current destination does not permit tag cancellation.
- If the value is missing, NULL, or unrecognized, do not assume cancellation is permitted or prohibited.

To evaluate cancellation permission, identify the tag's current destination, resolve it using the documented destination relationship, and inspect the `DestinationEpc` JSON of that destination. Do not assume `DestinationId` and `DestinationCode` are interchangeable.

Do not conclude that cancellation is prohibited solely because `Lock = 1`, or that cancellation is permitted solely because `Lock = 0` or `Freeze = 0`. Evaluate `AllowCancelTag` separately.

### 3.8 `tbl_DestinationType` — Warehouse/Destination Categories

Important columns:

- `fld_Id`
- `fld_DestinationTypeCode`
- `fld_DestinationTypeName`

This table represents the type or category of a physical destination (for example raw materials warehouse, finished goods warehouse, scrap yard, production line), not a physical warehouse itself.

The conceptual relationship is:

`tbl_Destination.DestinationType`  
→ `tbl_DestinationType.fld_DestinationTypeCode`

`tbl_Destination.DestinationType` is an integer while `fld_DestinationTypeCode` is stored as text; the relationship is logical. The provided DDL does not declare a primary key or unique constraint on `fld_DestinationTypeCode`. Confirm actual constraints before assuming uniqueness.

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

`ZoneCode` identifies a zone in the documented business relationship. `ZoneTitle` is the human-readable zone name. `ZoneStoreCode` stores the associated warehouse code. `ZoneCorridorId` relates the zone to its corridor.

The documented lookup paths are:

- Inventory zone: `tbl_Tags.TagZone` → `tbl_Zones.ZoneCode`
- Zone's warehouse: `tbl_Zones.ZoneStoreCode` → `tbl_Destination.DestinationCode`
- Zone's corridor: `tbl_Zones.ZoneCorridorId` → `tbl_Corridor.fld_CorridorId`

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

Hierarchy notes: `tbl_ProductGroup` is the top level; `tbl_ProductSubGroup` belongs to a group through `tbl_ProductSubGroup.fld_ProductGroupCode` → `tbl_ProductGroup.fld_ProductGroupCode`. `tbl_ProductType` is a product type tree (`ProductTypeParentId`, `ProductTypeParentsId`). `tbl_ProductStatus` defines quality/QC states (for example approved, rejected, pending).

#### Property A

`tbl_ProductPropertyA` includes:

- `fld_ProductPropertyAId`
- `fld_ProductPropertyATitle`
- `fld_ProductPropertyADesc`
- `fld_ProductPropertyAData`

Business meaning: production lines. When the user asks about a production line (خط تولید), use this table.

The provided DDL defines `fld_ProductPropertyATitle` as unique. It does not declare `fld_ProductPropertyAId` as an identity column.

#### Property B

`tbl_ProductPropertyB` includes:

- `fld_ProductPropertyBId`
- `fld_ProductPropertyBTitle`
- `fld_ProductPropertyBDesc`
- `fld_ProductPropertyBData`
- `fld_ProductPropertyAId`

Business meaning: work shifts (for example Shift 1, Shift 2, Night Shift). The shift working hours are stored in `fld_ProductPropertyBDesc` (for example `06:00-14:00`). When the user asks about a shift (شیفت کاری), use this table.

The `fld_ProductPropertyAId` column relates a shift (property B) to a production line (property A). Verify the actual constraints and application behavior before assuming that the relationship is enforced by a foreign key.

Property B is also used by the documented shift-based business-date logic. See Section 10.6.

#### Property C

`tbl_ProductPropertyC` includes:

- `fld_ProductPropertyCId`
- `fld_ProductPropertyCTitle`
- `fld_ProductPropertyCDesc`
- `fld_ProductPropertyCData`
- `fld_ProductPropertyCIdentity`
- `fld_ProductPropertyCTemp`

Business meaning: product sizes (for example Small, Medium, Large). When the user asks about the size (سایز) of a product or tag, use this table.

`fld_ProductPropertyCIdentity` is an identity-generated column (the table's primary key in the business model). It should not automatically be treated as the business key when relating property C to inventory records; the business key is `fld_ProductPropertyCId`.

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

The table also contains credential and personal-data columns (password hash, security stamp, e-mail, phone number, lockout and similar). These must never be selected in reports.

The supplied DDL declares `Id` as the primary key. It is an `nvarchar(128)` value, not an integer.

For operation-user lookup, the documented relationship is:

`tbl_MovementActions.MovementActionUserId`  
→ `tbl_User.Id`

The source definition gives `MovementActionUserId` as `nvarchar(max)` or `nvarchar(128)` depending on the table definition, so do not assume a physically enforced foreign key constraint.

Use `tbl_User.Name` for the operator's display name when this mapping applies. Use `Username` only when the request specifically requires the username or the application's business logic calls for it.

### 3.12 RFID Reader Logs — `tbl_UHFReaderLogHeader` and `tbl_UHF_ReaderLog`

`tbl_UHFReaderLogHeader` stores the hardware-level header of a bulk RFID scan event captured automatically by a UHF gate or station. It is the bridge between raw antenna reads and the validated warehouse actions in `tbl_MovementActions`.

Important columns: `fld_UHFReaderLogHeaderId` (identity), `fld_StationCode`, `fld_ActionType`, `fld_DocumentCode`, `fld_TruckCrossId`, `fld_UHFReaderLogHeaderUserId`, `fld_UHFReaderLogHeaderControlType`, `fld_CarProperties` (JSON about the scanned vehicle), `fld_HeaderUsedStatus`, `fld_MovementActionId`, `fld_HeaderCreateDateTime`.

`tbl_UHF_ReaderLog` stores raw, line-level RFID tag signals caught by reader antennas (every time an antenna detects a unit). It is a very large buffer table populated by the RFID middleware.

Important columns: `id`, `fld_TagSerial` (tag EPC that was read), `fld_ProductSerial`, `fld_Reader_Gate`, `fld_ReaderIp`, `fld_TagRead_DateTime` (Persian date text), `fld_TagRead_DateTimeMiladi` (Gregorian datetime), `fld_TagSelectedFlag`, `fld_InventoryId` (id of the parent header), `fld_Reader_GateType`, `fld_Desc`, `fld_ReaderDeviceType`, `ActionStatus`, `ActionDesc`, `fld_DocumentId`, `fld_WMUserId`, `fld_InventoryPackage`, `MovementActionId`, `fld_SaveUserId`.

Rules:

- Always filter by date using `fld_TagRead_DateTimeMiladi`. `fld_TagRead_DateTime` is a Persian date text (`nvarchar`); never pass it to `dbo.GeorgianDateToJalaliDate()`.
- Display `fld_TagRead_DateTimeMiladi` as Shamsi using `dbo.GeorgianDateToJalaliDate()`.
- A reader log row proves that a tag was physically read; it does not by itself prove a confirmed movement.

### 3.13 Documents — `tbl_DocumentHeader`, `tbl_DocumentItem`, `tbl_DocumentStatus`

`tbl_DocumentHeader` stores the master header of system documents (invoices, purchase orders, delivery permits) imported by API, Excel or manual entry.

Important columns: `fld_Id` (identity), `fld_DocumentKey` (business key), `fld_DocumentSaveUserId`, `fld_DocumentImportType`, `fld_DocumentImportFileName`, `fld_DocumentType`, `fld_DocumentType1`, `fld_DocumentType2`, `fld_DocumentImportDatetime`, `fld_DocumentDesc`, `fld_DocumentStatus`, `fld_DocumentHeaderData` (JSON), `fld_DocumentParent`, `fld_DocumentAggStatus`, `fld_DocumentDivideParent`, `fld_DocumentChangeStatusLastUserId`, `fld_DocumentCheckType`.

`fld_DocumentImportType` values: 0 = Excel, 1 = Api, 2 = Manual, 3 = Other, 4 = Aggregate, 5 = Divide.

`tbl_DocumentItem` stores the line items of a document: the products, requested quantities, or allowed quotas for warehouse execution.

Important columns: `fld_Id`, `fld_DocumentKey`, `fld_DocumentType`, `fld_DocumentType1`, `fld_DocumentType2`, `fld_DocumentItemProductCode`, `fld_DocumentItemProductTitle`, `fld_DocumentItemCount`, `fld_DocumentItemProducUnit`, `fld_DocumentItemsData` (JSON).

Always sum `fld_DocumentItemCount` when computing a document's target total.

`tbl_DocumentStatus` defines document states: `fld_DocumentStatusId`, `fld_DocumentStatusTitle`, `fld_DocumentStatusIsUpdatePermitted`, `fld_DocumentStatusIsCartablePermitted`. Filter header status by `fld_DocumentStatusTitle` (for example N'تایید شده' or N'موقت') through the join in Section 4.

Operation documents are linked to movements through the document key (`MovementActionDocumentId`, `RMovementActionDocumentId`, `fld_DocumentCode`, `fld_DocumentId` ↔ `fld_DocumentKey`). When the user asks for the owner/issuer of a document, join `tbl_User`.

### 3.14 Freeze — `tbl_FreezeHeader` and `tbl_FreezeItem`

`tbl_FreezeHeader` stores the header of an inventory freeze operation (a hold placed during audits, disputes or quality control). Columns: `fld_FreezeHeaderId`, `fld_FreezeUserId`, `fld_FreezeSaveDateTime`, `fld_FreezeDesc`, `fld_FreezeResult`.

`tbl_FreezeItem` lists the serials that belong to a freeze header. Columns: `fld_FreezeItemId`, `fld_FreezeProductSerial`, `fld_FreezeHeaderId`.

To identify which pallets/products belong to a freeze operation, join `tbl_FreezeItem` with `tbl_Tags` on the serial. When the user asks who froze a serial, when, or the description of the freeze, use `tbl_FreezeItem` → `tbl_FreezeHeader` → `tbl_User`. `fld_FreezeResult` expresses the overall state of the freeze operation; its exact value meaning is not further documented, so do not interpret its values beyond what the returned data states.

For the exit rule, the current `Freeze` flag on `tbl_Tags` is used (Section 3.1).

### 3.15 `tbl_Print` — Tag Print and Registration Log

`tbl_Print` is a flattened historical ledger of tag print/registration actions: which metadata was written to an EPC during print execution.

Important columns: `Id`, `ProductSerial`, `ProductCode`, `ProductName`, `ProductDescription`, `ProductType`, `ProductCount`, `ProductItemCount`, `ProductCountInPack`, `ProductUnit`, `ProductSize`, `ProductRegCode`, `ProductWeight`, `ProductVolume`, `ProductStatus`, `ProjectCode`, `TagEpc`, `ProductProductionShift`, `ProductProductionLine`, `ProductContractType`, `PackageId`, `Location`, `PrintActionId`, `PrintType`, `PrintActionDateTime`, `PrintUser`, `PrintFlag`, `RegisterActionDateTime`, `RegisterFlag`, `RegisterType`, `InputFileName`, `ErrorTime`, `ErrorDesc`, `SoftDelete`, `ReRegister`, `PrintQueue`, `DocumentId`, `DocumentItemId`, `offline_epc`, `offline_reg_status`, `offline_regDate`, `RegisterUser`, `RegisterDate`, `Manufacturer`, `AntiFire`, `DestinationCode`, `SoftDeleteUser`, `SoftDeleteDate`, `fld_ProductGroup`, `fld_ProductBrand`, `ProductProperties`, `fld_ProductSubGroup`, `fld_ProductClass`.

`SoftDelete = 1` marks removed log items. Time field: `PrintActionDateTime` (Gregorian datetime). `RegisterDate`, `offline_regDate` and `SoftDeleteDate` are text columns; do not convert them with the Gregorian-to-Shamsi function.

### 3.16 Truck Crossing Tables

`tbl_TruckCross` is the operational header of the truck entry/exit lifecycle. Column groups:

- Vehicle and driver: `fld_TruckCrossId` (bigint identity), `fld_TruckCrossPlaque`, `fld_TruckCrossInternationalPlaque`, `fld_TruckCrossDriverName`, `fld_TruckCrossDriverPhone`, `fld_TruckCrossNationalCode`, `fld_TruckCrossPassportCode`, `fld_TruckCrossSerial`, `fld_TruckCrossCompany`, `fld_TruckCrossType`, `fld_TruckCrossTypeDesc`, `fld_TruckCrossLicenseCode`, `fld_TruckCrossStatus`.
- Presence (arrival): `fld_TruckCrossPresentCause`, `fld_TruckCrossPresentTurn`, `fld_TruckCrossPresentDateTime`, `fld_TruckCrossPresentDesc`, `fld_TruckCrossPresentUserId`, `fld_TruckCrossPresentOperationType`, `fld_TruckCrossPresentOperationDestination`, `fld_TruckCrossPresentShipment`, `fld_TruckCrossPresentShipmentNumber`, `fld_TruckCrossPresentCustomer`, `fld_TruckCrossPresentRevokeDateTime`, `fld_TruckCrossPresentRevokeUserId`.
- Entry: `fld_TruckCrossEnterDateTime`, `fld_TruckCrossEnterWeightTonage`, `fld_TruckCrossEnterDesc`, `fld_TruckCrossEnterUserId`, `fld_TruckCrossEnterEpc`, `fld_TruckCrossEnterOtherEpcs`, `fld_TruckCrossEnterAcceptor`, `fld_TruckCrossEnterAcceptPlace`.
- Exit: `fld_TruckCrossExitDateTime`, `fld_TruckCrossExitDesc`, `fld_TruckCrossExitUserId`, `fld_TruckCrossExitWeightTonage`, `fld_TruckCrossExitGateId`, `fld_TruckCrossExitDestination`, `fld_TruckCrossExitPureWeightCargo`, `fld_TruckCrossExitWeightbridgeReceiptNumber`, `fld_TruckCrossExitCargoOwnerName`, `fld_TruckCrossExitCargoOwnerPhone`, `fld_TruckCrossExitDeliveryAddress`, `fld_TruckCrossExitShipmentCost`, `fld_TruckCrossExitPaymentType`, `fld_TruckCrossExitUnitPrice`, `fld_TruckCrossExitTotalCost`, `fld_TruckCrossExitDistance`.
- Dynamic data: `fld_TruckCrossDynamicFields` (JSON).

Reference tables (each has an id and a title column): `tbl_TruckCompany` (`fld_TruckCompanyId`, `fld_TruckCompanyTitle`), `tbl_TruckType` (`fld_TruckTypeId`, `fld_TruckTypeTitle`), `tbl_TruckCrossCause` (`fld_TruckCrossCauseId`, `fld_TruckCrossCauseTitle`, `fld_TruckCrossCauseEnterActionTypeId`, `fld_TruckCrossCauseExitActionTypeId`), `tbl_TruckCrossOperationType` (`fld_TruckCrossOperationTypeId`, `fld_TruckCrossOperationTypeTitle`, `TruckCrossCauseId`), `tbl_TruckCrossOperationDestination` (`fld_TruckCrossOperationDestinationId`, `fld_TruckCrossOperationDestinationTitle`), `tbl_TruckCrossShipment` (`fld_TruckCrossShipmentId`, `fld_TruckCrossShipmentTitle`), `tbl_TruckCrossCustomer` (`fld_TruckCrossCustomerId`, `fld_TruckCrossCustomerTitle`), `tbl_TruckCrossAcceptPlace` (`fld_TruckCrossAcceptPlaceId`, `fld_TruckCrossAcceptPlaceTitle`), `tbl_TruckCrossProductType` (`fld_TruckCrossProductTypeId`, `fld_TruckCrossProductTypeTitle`, `TruckCrossCauseIdsArray`).

`tbl_TruckCrossItem` holds the line items of a crossing: `fld_TruckCrossItemId`, `fld_TruckCrossItemTitle`, `Type`, `ProductUnit`, `ProductCount`, `ProductSerial`, `ProductCode`, `TruckCrossProductTypeId`, `TruckCrossId`.

`tbl_TruckCrossShippingFee` holds pricing rules: `fld_TruckCrossShippingFeeId`, `CompanyId`, `CustomerId`, `ProductTypeId`, `ShipmentId`, `FromDate`, `ToDate`, `FeeStatus`, `FeeAmount`, `FeeWeight`, `FeeDistance`.

Personal-data columns (national code, passport code, driver phone, cargo owner phone, driver mobile on movements) must be selected only when the user explicitly asks for them.

### 3.17 `tbl_Corridor` — Warehouse Corridors

Corridors (aisles) group storage zones inside a warehouse. Columns: `fld_CorridorId`, `fld_CorridorWarehouseCode` (the warehouse's `DestinationCode`), `fld_CorridorName`, `fld_CorridorDirection`, `fld_CorridorVerticalOrder`, `fld_CorridorHorizontalOrder`, `fld_CorridorWidth`, `fld_CorridorZoom`, `fld_CorridorIsFaken`. A corridor contains multiple zones in `tbl_Zones`.

### 3.18 Dynamic Fields — `tbl_DynamicFields` and `tbl_DynamicFieldSections`

`tbl_DynamicFields` stores the metadata definitions of dynamic (custom) fields attached to transactions, vehicles, documents, or tags. Columns: `fld_DynamicFieldId`, `fld_DynamicFieldUser`, `fld_DynamicFieldTitle`, `fld_DynamicFieldType`, `fld_IsSystematicField`, `fld_IsHeaderKey`, `fld_DynamicFieldDateTime`, `fld_DynamicFieldRelatedTitle1`, `fld_DynamicFieldRelatedTitle2`, `fld_DynamicFieldRelatedTitle3`, `fld_DynamicFieldActionType`, `fld_DynamicFieldShowColumn`, `fld_DynamicFieldShowColumnForAction`, `fld_DynamicFieldDocGroupAggregate`, `fld_DynamicFieldValueType`, `fld_DynamicFieldDefaultValue`, `fld_DynamicFieldValueOptions`, `fld_DynamicFieldRequirement`, `fld_DynamicFieldOrder`, `fld_DynamicFieldSectionId`, `fld_DynamicFieldParentId`, `fld_DynamicFieldIsReadOnly`.

`tbl_DynamicFieldSections` groups fields: `fld_DfSectionId`, `fld_DfSectionTitle`, `fld_DfType`.

Values entered for dynamic fields are typically stored inside the JSON columns (`ProductProperties`, `MovementActionData`, and similar). The definition table tells which custom fields exist, but do not assume that a field title equals a JSON key without verification (see Section 8.8).

### 3.19 Product Expiry and Gallery

`tbl_ProductExpire` stores expiration and warranty activation information: `fld_ProductExpireId`, `fld_ProductExpireProductSerial`, `fld_ProductExpireProductCode`, `fld_ProductExpireStatus`, `fld_ProductExpireStartDate`, `fld_ProductExpireEndDate`, `fld_ProductExpireActivationType`, `fld_ProductExpireLastModifiedDateTime`, `fld_ProductExpireLastModifiedUserId`. Start and end dates are Persian date strings (`nvarchar(10)`); do not convert them with the Gregorian-to-Shamsi function.

`tbl_Gallery` stores uploaded media files: `fld_GalleryId`, `fld_GalleryUserId`, `fld_GalleryMediaName`, `fld_GalleryMediaPath`, `fld_GalleryUsageType`, `fld_GalleryUploadDateTime`, `fld_GalleryUsageId`, `fld_GalleryMediaExtensionType`, `fld_GalleryAdditionalData`. Product images are linked through `tbl_Products.ProductGalleryId`.

### 3.20 Notifications

`tbl_NotificationEventTypes` (`Id`, `fld_NETitle`, `fld_NECommand`) defines notification event templates. `tbl_NotificationOrders` (`Id`, `fld_NOId`, `fld_NOStatus`, `fld_NODateTime`, `fld_NOUserId`, `fld_NOType`, `fld_NOTitle`, `fld_NOEventType`, `fld_NOTimePeriod`, `fld_NOSendDay`, `fld_NOSendClock`, `fld_NOSendType`, `fld_NOSendContacts`, `fld_NOContent`) defines scheduled notification orders. `tbl_NotificationQueue` (`fld_Id`, `fld_Text`, `fld_SendType`, `fld_Contact`, `fld_SendDateTime`, `fld_SendDate`, `fld_SendTime`, `fld_SendStatus`, `fld_NotificationOrderId`, `fld_QueueActionCode`, `fld_SaveDateTime`) holds the individual outgoing messages. One order may generate multiple queue records.

### 3.21 `tbl_Province`

`tbl_Province` (`fld_Id`, `fld_Name`) defines geographical provinces. Its child `tbl_City` has no available definition.

---

## 4. Lookup Mapping: Resolve User-Friendly Names Correctly

When a user asks for records by a name or title, use the relevant lookup table rather than comparing the user's text directly to a code or ID.

The following mapping describes the documented business relationships.

**Display rule:** whenever a tag or product attribute is resolved through one of these lookups, the answer shows the TITLE/NAME column only, never the code or ID (see Section 4.5).

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
| Tag Registrar (ثبت‌کننده تگ) | `tbl_Tags.TagRegisterUser` | `tbl_User` | `TagRegisterUser = Id` | `Name` |
| Operation Type (نوع عملیات) | `tbl_MovementActions.MovementActionTp` | `tbl_ActionTypes` | `MovementActionTp = fld_ActionTypeId` | `fld_ActionTypeTitle` |
| Station / Gate (گیت) | `tbl_MovementActions.MovementActionUHFLogGate` | `tbl_Station` | `MovementActionUHFLogGate = fld_StationCode` | `fld_StationName` |
| User / Operator (کاربر / اپراتور) | `tbl_MovementActions.MovementActionUserId` | `tbl_User` | `MovementActionUserId = Id` | `Name` |
| Warehouse Type (نوع انبار) | `tbl_Destination.DestinationType` | `tbl_DestinationType` | `DestinationType = fld_DestinationTypeCode` | `fld_DestinationTypeName` |
| Corridor (راهرو) | `tbl_Zones.ZoneCorridorId` | `tbl_Corridor` | `ZoneCorridorId = fld_CorridorId` | `fld_CorridorName` |
| Document Status (وضعیت سند) | `tbl_DocumentHeader.fld_DocumentStatus` | `tbl_DocumentStatus` | `fld_DocumentStatus = fld_DocumentStatusId` | `fld_DocumentStatusTitle` |
| Document Registrant (ثبت‌کننده سند) | `tbl_DocumentHeader.fld_DocumentSaveUserId` | `tbl_User` | `fld_DocumentSaveUserId = Id` | `Name` |
| Reader Log Station (گیت اسکن) | `tbl_UHFReaderLogHeader.fld_StationCode` | `tbl_Station` | `fld_StationCode = fld_StationCode` | `fld_StationName` |

### 4.1 Filtering by Names

When the user specifies a Persian or English name, filter using the corresponding display/name column, usually with `LIKE` when partial matching is appropriate.

For example, a request for a warehouse by name should be resolved through `tbl_Destination.DestinationTitle`, not by comparing the Persian name directly against `tbl_Tags.TagInDestinationId`. When the user asks about any specific warehouse or location by name (for example «انبار محصول»), join `tbl_Tags` with `tbl_Destination` on `TagInDestinationId = DestinationCode` and filter on `DestinationTitle`.

The same principle applies to product, zone, brand, group, subgroup, class, quality, product type, operation type, station, user, and warehouse type.

Use an exact comparison only when the user requests an exact match or the business requirement explicitly requires one.

Properties that are stored directly as text or numbers on `tbl_Tags` (for example `ProductName`, `ProductCount`) do not need a lookup join; filter them directly on `tbl_Tags`.

### 4.2 Important Limitations

The mapping above is a business-key map. It does not prove that every relationship has a corresponding SQL Server foreign key constraint.

The supplied schema also contains type differences and columns whose uniqueness is not declared. Before assuming a join returns exactly one lookup record, verify the referenced code's uniqueness and the actual application behavior.

For example, the DDL for `tbl_ProductType` declares `ProductTypeCode` as nullable and does not declare it unique. A join through this column may therefore require further validation to prevent duplicate result rows.

### 4.3 Lookups When the Query Starts From `tbl_Products`

`tbl_Products` has its own classification columns. Use these exact column names (they are different from the column names of the lookup tables):

| User's Request | Reference Column | Lookup Table | Join Condition | Display/Filter Column |
|---|---|---|---|---|
| Brand | `tbl_Products.fld_ProductBrand` | `tbl_ProductBrand` | `fld_ProductBrand = fld_ProductBrandCode` | `fld_ProductBrandTitle` |
| Group | `tbl_Products.fld_ProductGroup` | `tbl_ProductGroup` | `fld_ProductGroup = fld_ProductGroupCode` | `fld_ProductGroupTitle` |
| Subgroup | `tbl_Products.fld_ProductSubGroup` | `tbl_ProductSubGroup` | `fld_ProductSubGroup = fld_ProductSubGroupCode` | `fld_ProductSubGroupTitle` |
| Class | `tbl_Products.fld_ProductClass` | `tbl_ProductClass` | `fld_ProductClass = fld_ProductClassCode` | `fld_ProductClassTitle` |
| Product Type | `tbl_Products.ProductType` | `tbl_ProductType` | `ProductType = ProductTypeCode` | `ProductTypeTitle` |
| Quality Status | `tbl_Products.ProductStatus` | `tbl_ProductStatus` | `ProductStatus = ProductStatusCode` | `ProductStatusTitle` |
| Size | `tbl_Products.ProductSize` | `tbl_ProductPropertyC` | `ProductSize = fld_ProductPropertyCId` | `fld_ProductPropertyCTitle` |
| Registrant | `tbl_Products.ProductRegUser` | `tbl_User` | `ProductRegUser = Id` | `Name` |
| Gallery image | `tbl_Products.ProductGalleryId` | `tbl_Gallery` | `ProductGalleryId = fld_GalleryId` | `fld_GalleryMediaName` |

Other documented hierarchy lookups: `tbl_ProductSubGroup.fld_ProductGroupCode` → `tbl_ProductGroup.fld_ProductGroupCode`; `tbl_ProductPropertyB.fld_ProductPropertyAId` → `tbl_ProductPropertyA.fld_ProductPropertyAId`.

### 4.4 Truck Crossing Lookups

| User's Request | Reference Column | Lookup Table | Join Condition | Display/Filter Column |
|---|---|---|---|---|
| Transport company | `tbl_TruckCross.fld_TruckCrossCompany` | `tbl_TruckCompany` | `= fld_TruckCompanyId` | `fld_TruckCompanyTitle` |
| Truck type | `tbl_TruckCross.fld_TruckCrossType` | `tbl_TruckType` | `= fld_TruckTypeId` | `fld_TruckTypeTitle` |
| Presence cause | `tbl_TruckCross.fld_TruckCrossPresentCause` | `tbl_TruckCrossCause` | `= fld_TruckCrossCauseId` | `fld_TruckCrossCauseTitle` |
| Operation type | `tbl_TruckCross.fld_TruckCrossPresentOperationType` | `tbl_TruckCrossOperationType` | `= fld_TruckCrossOperationTypeId` | `fld_TruckCrossOperationTypeTitle` |
| Operation destination | `tbl_TruckCross.fld_TruckCrossPresentOperationDestination` | `tbl_TruckCrossOperationDestination` | `= fld_TruckCrossOperationDestinationId` | `fld_TruckCrossOperationDestinationTitle` |
| Shipment | `tbl_TruckCross.fld_TruckCrossPresentShipment` | `tbl_TruckCrossShipment` | `= fld_TruckCrossShipmentId` | `fld_TruckCrossShipmentTitle` |
| Customer | `tbl_TruckCross.fld_TruckCrossPresentCustomer` | `tbl_TruckCrossCustomer` | `= fld_TruckCrossCustomerId` | `fld_TruckCrossCustomerTitle` |
| Accept place | `tbl_TruckCross.fld_TruckCrossEnterAcceptPlace` | `tbl_TruckCrossAcceptPlace` | `= fld_TruckCrossAcceptPlaceId` | `fld_TruckCrossAcceptPlaceTitle` |
| Item product type | `tbl_TruckCrossItem.TruckCrossProductTypeId` | `tbl_TruckCrossProductType` | `= fld_TruckCrossProductTypeId` | `fld_TruckCrossProductTypeTitle` |

### 4.5 Mandatory Display Rule: Show Titles, Never Codes or IDs

This rule applies to every question about a tag (`tbl_Tags`) or a product (`tbl_Products`), whether the question asks for one attribute or for a full report.

1. When the requested attribute is stored as a code/ID that references another table (Tables 4.5.1 and 4.5.2), JOIN the lookup table using the documented key and return ONLY the title/name column. Never return the code, the ID, or the raw column value in place of the title.
2. Return a code only when the user explicitly asks for the code or identifier (for example «کد برند», «کد سایز»). When the user asks for both, return the title and the code as separate columns with different Persian aliases.
3. The rule applies also inside lists and detail reports (for example «مشخصات سریال X», «لیست کالاها»): resolve EVERY lookup attribute that appears in the output. Do not mix titles and raw codes in the same result.
4. Use `LEFT JOIN` so that the tag/product row is kept. If no title exists, the title is NULL; never substitute the code for a missing title.
5. Filtering and grouping by such an attribute use its title column (`LIKE` for names, `GROUP BY` the title).
6. The Persian output alias names the attribute, not the code: for example `[سایز]`, `[نوع کالا]`, `[برند]`, `[گروه کالا]`, `[کیفیت کالا]`, `[انبار]`.
7. Choose the starting table by what the user gives: a product code or product name (کالا) → start from `tbl_Products` and use Table 4.5.2; a serial or EPC (سریال / تگ) → start from `tbl_Tags`. For a serial/EPC, the attributes of the tag's product definition (type, quality, group, subgroup, brand, class, size) are resolved by default through the MASTER product: join `tbl_Products` on `tbl_Tags.ProductCode = tbl_Products.ProductCode`, then the lookup of Table 4.5.2. Use the value stored directly on `tbl_Tags` (last column of Table 4.5.1) only when the user explicitly asks about the value recorded on the tag itself. Attributes that exist only on the tag (production line, shift, warehouse, warehouse type, zone, registrar) use the tag columns of Table 4.5.1. Do not silently substitute a tag-stored value for a master value or vice versa.

#### Table 4.5.1 — attributes asked about a tag/serial (alias `t`)

Attributes of the tag's product definition (default source = master product `p`, joined with `LEFT JOIN tbl_Products p ON t.ProductCode = p.ProductCode`, then the lookup of Table 4.5.2):

| Attribute asked (Persian) | Default path (master product) | Title column to return | Tag-stored alternative (only when the user asks about the value recorded on the tag) |
|---|---|---|---|
| نام کالا | `p.ProductCode` (join above) | `p.ProductTitle` | `t.ProductName` is the name stored on the tag |
| نوع کالا | `p.ProductType = pt.ProductTypeCode` | `pt.ProductTypeTitle` | `t.ProductType = pt.ProductTypeCode` |
| کیفیت / وضعیت کیفی | `p.ProductStatus = ps.ProductStatusCode` | `ps.ProductStatusTitle` | `t.ProductStatus = ps.ProductStatusCode` |
| گروه کالا | `p.fld_ProductGroup = pg.fld_ProductGroupCode` | `pg.fld_ProductGroupTitle` | `t.fld_ProductGroup = pg.fld_ProductGroupCode` |
| زیرگروه کالا | `p.fld_ProductSubGroup = psg.fld_ProductSubGroupCode` | `psg.fld_ProductSubGroupTitle` | `t.fld_ProductSubGroup = psg.fld_ProductSubGroupCode` |
| برند | `p.fld_ProductBrand = pb.fld_ProductBrandCode` | `pb.fld_ProductBrandTitle` | `t.fld_ProductBrand = pb.fld_ProductBrandCode` |
| طبقه کالا | `p.fld_ProductClass = pc.fld_ProductClassCode` | `pc.fld_ProductClassTitle` | `t.fld_ProductClass = pc.fld_ProductClassCode` |
| سایز | `p.ProductSize = psz.fld_ProductPropertyCId` | `psz.fld_ProductPropertyCTitle` | `t.fld_ProductPropertyCId = psz.fld_ProductPropertyCId` |

Attributes that exist only on the tag (always from `tbl_Tags`):

| Attribute asked (Persian) | Column in `tbl_Tags` | Join (use `LEFT JOIN`) | Title column to return |
|---|---|---|---|
| خط تولید | `fld_ProductPropertyAId` | `tbl_ProductPropertyA pa ON t.fld_ProductPropertyAId = pa.fld_ProductPropertyAId` | `pa.fld_ProductPropertyATitle` |
| شیفت کاری | `fld_ProductPropertyBId` | `tbl_ProductPropertyB pb2 ON t.fld_ProductPropertyBId = pb2.fld_ProductPropertyBId` | `pb2.fld_ProductPropertyBTitle` |
| انبار | `TagInDestinationId` | `tbl_Destination d ON t.TagInDestinationId = d.DestinationCode` | `d.DestinationTitle` |
| نوع انبار | through the warehouse | `tbl_DestinationType dt ON d.DestinationType = dt.fld_DestinationTypeCode` (after joining `d`) | `dt.fld_DestinationTypeName` |
| محل / ناحیه | `TagZone` | `tbl_Zones z ON t.TagZone = z.ZoneCode` | `z.ZoneTitle` |
| ثبت‌کننده تگ | `TagRegisterUser` | `tbl_User u ON t.TagRegisterUser = u.Id` | `u.Name` |

Columns of `tbl_Tags` with no documented lookup (return the stored value only when the user asks for them; never invent a join): `TagStatus`, `TagInDestinationId2`, `TagInActionId2`, `fld_LastModifierUser`, `ContractStatus`, `RegCode`, `ProjectCode`.

#### Table 4.5.2 — `tbl_Products` attributes (alias `p`)

| Attribute asked (Persian) | Column in `tbl_Products` | Join (use `LEFT JOIN`) | Title column to return |
|---|---|---|---|
| نوع کالا | `ProductType` | `tbl_ProductType pt ON p.ProductType = pt.ProductTypeCode` | `pt.ProductTypeTitle` |
| کیفیت / وضعیت کیفی | `ProductStatus` | `tbl_ProductStatus ps ON p.ProductStatus = ps.ProductStatusCode` | `ps.ProductStatusTitle` |
| گروه کالا | `fld_ProductGroup` | `tbl_ProductGroup pg ON p.fld_ProductGroup = pg.fld_ProductGroupCode` | `pg.fld_ProductGroupTitle` |
| زیرگروه کالا | `fld_ProductSubGroup` | `tbl_ProductSubGroup psg ON p.fld_ProductSubGroup = psg.fld_ProductSubGroupCode` | `psg.fld_ProductSubGroupTitle` |
| برند | `fld_ProductBrand` | `tbl_ProductBrand pb ON p.fld_ProductBrand = pb.fld_ProductBrandCode` | `pb.fld_ProductBrandTitle` |
| طبقه کالا | `fld_ProductClass` | `tbl_ProductClass pc ON p.fld_ProductClass = pc.fld_ProductClassCode` | `pc.fld_ProductClassTitle` |
| سایز | `ProductSize` | `tbl_ProductPropertyC psz ON p.ProductSize = psz.fld_ProductPropertyCId` | `psz.fld_ProductPropertyCTitle` |
| ثبت‌کننده کالا | `ProductRegUser` | `tbl_User u ON p.ProductRegUser = u.Id` | `u.Name` |
| تصویر کالا | `ProductGalleryId` | `tbl_Gallery g ON p.ProductGalleryId = g.fld_GalleryId` | `g.fld_GalleryMediaName` |

Columns of `tbl_Products` with no documented lookup (return the stored value only when asked; never invent a join or a meaning): `ProductUnit`, `fld_ProductGuaranteeType`, `fld_ProductExpireType`.

#### Examples

Size of a product (user gives a product code):

<<Search
SELECT
    p.ProductTitle AS [نام کالا],
    psz.fld_ProductPropertyCTitle AS [سایز]
FROM tbl_Products AS p
LEFT JOIN tbl_ProductPropertyC AS psz
    ON p.ProductSize = psz.fld_ProductPropertyCId
WHERE p.ProductCode = N'0110185747001'
>>

Product type of a serial (user gives a serial; default master-product path):

<<Search
SELECT
    t.ProductSerial AS [سریال کالا],
    pt.ProductTypeTitle AS [نوع کالا]
FROM tbl_Tags AS t
LEFT JOIN tbl_Products AS p
    ON t.ProductCode = p.ProductCode
LEFT JOIN tbl_ProductType AS pt
    ON p.ProductType = pt.ProductTypeCode
WHERE t.ProductSerial = N'10000019'
>>

Brand, group and subgroup of a product:

<<Search
SELECT
    p.ProductTitle AS [نام کالا],
    pb.fld_ProductBrandTitle AS [برند],
    pg.fld_ProductGroupTitle AS [گروه کالا],
    psg.fld_ProductSubGroupTitle AS [زیرگروه کالا]
FROM tbl_Products AS p
LEFT JOIN tbl_ProductBrand AS pb ON p.fld_ProductBrand = pb.fld_ProductBrandCode
LEFT JOIN tbl_ProductGroup AS pg ON p.fld_ProductGroup = pg.fld_ProductGroupCode
LEFT JOIN tbl_ProductSubGroup AS psg ON p.fld_ProductSubGroup = psg.fld_ProductSubGroupCode
WHERE p.ProductCode = N'0110185747001'
>>

In these examples the product code and the serial appear only in the `WHERE` condition (they are what the user supplied). Every attribute that references a lookup table is shown as its title.

---

## 5. Entity Resolution — Choosing the Correct Starting Table

Select the base table according to the user's intent. Do not begin every query from `tbl_Tags`.

### 5.1 Master Product Data (اطلاعات پایه کالا)

If the user asks about product definitions, technical codes, technical specifications, packaging, product properties, or master product information, start with:

`tbl_Products`

Use `tbl_Tags` only when the request also requires inventory/tag information.

When the answer includes classification attributes (type, group, brand, size and so on), show their titles through the joins of Section 4.5.2, never their codes.

### 5.2 Current Inventory and Tags (موجودی فعلی)

If the user asks about registered tags, current stock, current warehouse location, or the current state of tagged items, start with:

`tbl_Tags`

Use the appropriate lookup tables when the user asks for human-readable names.

When the answer includes classification, location or user attributes of the tag, always show their titles through the joins of Section 4.5.1, never their codes or IDs.

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

When item-level lines are needed, join `tbl_TagsMovement` from the header using the path that matches the intent: `tbl_MovementActions.MovementActionId = tbl_TagsMovement.RMovementActionId` returns only the entry/registration lines; `tbl_MovementActions.MovementActionId = tbl_TagsMovement.HMovementActionId` returns only the exit/return lines. Joining only one path silently omits the other path's lines.

### 5.6 RFID Reader Logs (لاگ خوانش RFID)

If the user asks whether a tag was physically detected by an RFID reader, antenna, or gate, start with:

`tbl_UHF_ReaderLog`

This is distinct from movement history. A movement record does not, by itself, prove that a specific RFID read occurred.

The documented EPC relationship is:

`tbl_Tags.TagEpc` ↔ `tbl_UHF_ReaderLog.fld_TagSerial`

The reader log also stores the product serial in `fld_ProductSerial` (→ `tbl_Tags.ProductSerial`). Filter dates with `fld_TagRead_DateTimeMiladi` (Section 3.12). When the scan batch (header) is needed, join `tbl_UHFReaderLogHeader` on `tbl_UHF_ReaderLog.fld_InventoryId = tbl_UHFReaderLogHeader.fld_UHFReaderLogHeaderId`.

### 5.7 Truck and Vehicle Crossings (تردد کامیون / خودرو)

If the user asks about trucks, vehicle plates, drivers, shipments, or truck-crossing records, start with:

`tbl_TruckCross`

If the question concerns a movement operation associated with a truck crossing, use:

`tbl_MovementActions.MovementActionTruckCrossId`  
→ `tbl_TruckCross.fld_TruckCrossId`

Both columns are `bigint`. Use the lookup tables of Section 4.4 for names/titles, and `tbl_TruckCrossItem.TruckCrossId = tbl_TruckCross.fld_TruckCrossId` for line items. Do not select personal-data columns unless the user explicitly asks for them.

### 5.8 Handheld Inventory Counting (انبارگردانی دستی)

If the user asks about manual inventory counting performed using handheld devices (دستگاه هندهلد), the business concept is represented by:

`tbl_InventoryTags`

The column definition of this table is not available (see Section 2, Availability). Do not generate a query for it. Do not automatically treat this as the same process as physical counting.

### 5.9 Physical Counting (شمارش فیزیکی)

Physical counting (شمارش فیزیکی) is a separate business concept from handheld inventory counting (انبارگردانی دستی).

It may involve other reader devices, physical-counting workflows, or Excel-based processes.

Do not automatically map a request about physical counting to `tbl_InventoryTags` unless the application's actual workflow establishes that relationship.

### 5.10 Placement Missions (ماموریت جانمایی)

If the user asks about product/tag placement or movement between storage zones, the business concept is represented by:

`tbl_ProductPlacementMissions`

The documented relationship is:

`tbl_ProductPlacementMissions.fld_PPMProductSerial`  
→ `tbl_Tags.ProductSerial`

The column definition of this table is not available (see Section 2, Availability). Do not generate a query for it.

### 5.11 Diagnosing Operation Denial and Clarifying Missing Information

When the user asks why an operation cannot be performed on a product, serial, or tag, first determine whether the available information is sufficient to identify the exact entity, requested operation, and conditions under which the operation should be performed.

The objective is to identify the actual reason for the problem based on the current database state, the relevant operation type, and documented business rules—not to list generic possible causes.

#### 5.11.1 Ask the User for Missing Information Before Generating a Query

If the available information is insufficient to generate a correct query or identify the requested operation reliably, ask the user a specific clarifying question before generating the query.

Questions must be relevant to the current ambiguity. Do not ask for information that is already available, and do not ask every possible question at once. First evaluate the information already provided, then ask only for the missing details necessary to continue.

Depending on the request, the required information may include:

- **Product or tag identifier:** The product serial number, tag EPC, or another identifier required to identify the correct record.
- **Requested operation:** The name or description of the movement operation the user wants to perform.
- **Source destination:** The warehouse or destination where the product or tag is currently located, or from which the operation should begin.
- **Target destination:** The warehouse or destination to which the product or tag is intended to be moved.
- **Operation-specific conditions:** Any additional information required by documented business rules to identify the correct operation or evaluate its eligibility.

These are examples, not mandatory questions for every request. Ask only for the information necessary to resolve the current issue.

If the user does not know the operation name but provides the source and target destinations, use those details to identify the relevant operation type whenever the documented relationships and available data allow it. Do not require the user to provide an operation name that can be resolved from the database.

If the source or target destination is unknown and is necessary to identify the correct operation type, ask the user for the missing destination.

#### 5.11.2 Identify the Operation Type Using the Source and Target Destinations

If the user wants to perform a movement operation but has not specified its exact operation type, use the provided source and target destinations to identify the matching operation.

Follow this general investigation pattern:

1. Extract the source and target destination names from the user's request.
2. Resolve those names to the corresponding destination records using documented lookup mappings and relationships.
3. Retrieve the valid destination keys required for matching the operation type.
4. Identify the operation type that matches the resolved source and target conditions using the documented relationships with `tbl_ActionTypes`.
5. Retrieve and evaluate the active controls associated with the identified operation type.
6. Retrieve the current state of the relevant serial or tag and evaluate whether it satisfies the applicable controls.
7. Report the result based on the actual database records and documented business rules.

The destination names provided by the user may be display titles, while database relationships may depend on stored keys. Resolve user-provided names to the correct records first, then use the documented keys to join the relevant tables. Do not join tables using display names when a documented key-based relationship is available.

When identifying a movement operation, examine the documented mappings for these fields in `tbl_ActionTypes`:

- `fld_ActionTypeFromDestinationType`
- `fld_ActionTypeToTypeDestinationType`

Use the knowledge base's documented definitions and relationship mappings to determine how these fields correspond to the source and target destination information. Do not infer the exact relationship or expected key values from the field names alone.

If multiple operation types match the available conditions, do not select one based only on a similar name. If the correct operation cannot be determined reliably from the available data, ask the user for the specific additional information needed to distinguish the operations.

#### 5.11.3 Evaluate Whether the Serial or Tag Is Eligible for the Operation

After identifying the correct operation type, retrieve the current state of the serial or tag from `tbl_Tags` and the relevant operation controls from `tbl_ActionTypes`.

Evaluate only controls for which the actual value, documented meaning, and applicability to the requested operation are established.

**For movement operations:**

- Check `Lock` and determine whether the documented control related to blocking after a failed goods inspection applies to the requested operation.
- Evaluate `Freeze` together with the `Freezed` control configured for the operation and the relevant source and target conditions.
- Evaluate other active controls only according to their documented meanings and applicability.
- Do not treat an active control as proof that the serial or tag violates that control. Verify the current serial or tag state against the actual condition enforced by the control.

**For cancellation operations:**

- Identify the tag's current destination.
- Resolve the destination using the documented relationship.
- Check the value of `AllowCancelTag` in the `DestinationEpc` JSON field of that destination record.
- Do not infer cancellation permission from `Lock`, `Freeze`, or movement-operation controls.

#### 5.11.4 Decision Rules When Information Is Missing

- If the serial or tag identifier is missing and the correct record cannot otherwise be identified, ask the user for the required identifier.
- If the requested operation is unclear and cannot be determined from the available information, ask the user to specify the operation or describe the intended action.
- If the source or target destination is necessary to identify the operation type but is unknown, ask for the missing destination.
- If the user's information is sufficient to identify the entity but the required database mapping or relationship is not documented, do not guess the relationship or key. Identify the limitation and request additional information if it can resolve the ambiguity.
- If sufficient information is already available to generate a valid query, proceed without asking unnecessary questions.
- If the investigation does not establish the cause, do not invent a possible cause or attribute undocumented behavior to the system.

#### 5.11.5 Reporting the Investigation Result

The final answer must report only conclusions supported by the retrieved data and documented business rules.

When the cause is identified, explain it in clear, user-facing language and refer to the actual relevant state or condition. Do not expose internal table names, column names, or database implementation details in ordinary user-facing answers unless the user explicitly requests technical details.

If the data establishes that a control imposes a requirement but does not establish that the serial or tag violates that requirement, explain the requirement without presenting it as the confirmed cause.

If the investigation cannot proceed because information is missing, ask the next specific question needed to continue. If the investigation is blocked by an undocumented relationship or mapping, clearly state the limitation rather than guessing.

Do not list generic possible causes, guess the operation type, invent database relationships, or declare a cause without supporting evidence.

### 5.12 Documents (اسناد)

If the user asks about documents, document status, document items, or imported documents, start with `tbl_DocumentHeader` and join `tbl_DocumentItem` through `fld_DocumentKey`, and `tbl_DocumentStatus` through `fld_DocumentStatus = fld_DocumentStatusId` when the status title is needed. See Section 3.13.

### 5.13 Freeze Operations (فریز)

If the user asks which serials are frozen by a freeze operation, who froze them, or when, start with `tbl_FreezeItem`, join `tbl_FreezeHeader` through `fld_FreezeHeaderId`, `tbl_Tags` through `fld_FreezeProductSerial = ProductSerial`, and `tbl_User` through `fld_FreezeUserId = Id`. See Section 3.14.

### 5.14 Print and Registration Log (چاپ و ثبت تگ)

If the user asks about tag printing or registration history, start with `tbl_Print`. Exclude soft-deleted rows only when the user asks for non-deleted records (`SoftDelete` meaning is in Section 3.15).

### 5.15 Other Subjects

- Corridors and warehouse layout: `tbl_Destination` → `tbl_Corridor` → `tbl_Zones`.
- Product expiry and warranty: `tbl_ProductExpire` (and the guarantee/expire columns of `tbl_Products`).
- Notifications: `tbl_NotificationOrders` → `tbl_NotificationQueue`.
- Dynamic field definitions: `tbl_DynamicFields` and `tbl_DynamicFieldSections`.

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

`tbl_TagsMovement.ProductCode` also relates directly to `tbl_Products.ProductCode`.

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

### 6.4 Movement, Documents, Reader Logs and Trucks

A movement action connects to other process records as follows:

- Document: `tbl_MovementActions.MovementActionDocumentId` and `tbl_TagsMovement.RMovementActionDocumentId` carry the document key (→ `tbl_DocumentHeader.fld_DocumentKey`).
- RFID scan: `tbl_UHFReaderLogHeader.fld_MovementActionId` and `tbl_UHF_ReaderLog.MovementActionId` → `tbl_MovementActions.MovementActionId`.
- Truck crossing: `tbl_MovementActions.MovementActionTruckCrossId` → `tbl_TruckCross.fld_TruckCrossId`.
- Gate/station: `tbl_MovementActions.MovementActionUHFLogGate` → `tbl_Station.fld_StationCode`.

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

### 7.3 Warehouse Layout Chain

`tbl_Destination` (warehouse, `DestinationCode`)  
← `tbl_Corridor.fld_CorridorWarehouseCode`  
← `tbl_Zones.ZoneCorridorId = tbl_Corridor.fld_CorridorId`  
← `tbl_Tags.TagZone = tbl_Zones.ZoneCode`

`tbl_Zones.ZoneStoreCode = tbl_Destination.DestinationCode` is the direct warehouse of a zone.

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

### 8.4 Operation Type Active Controls

Source: `tbl_ActionTypes.fld_ActionTypeActiveControls`

Example value:

{"NotInSourceIgnore":"false","NotInSourceError":"true","Freezed":"false","FailedQC":"false","NotQc":"false","NotRegisterIgnore":"false","NotRegisterError":"true","TagLifeLessThanIgnore":"false","DocCodeSet":"false","DocItemsCheck1":"false","DocItemsCheck2":"false","DocItemsCheck3":"false"}

The values are strings, not booleans. A control is active only when its value is the string "true". Extract a key with:

`JSON_VALUE(act.fld_ActionTypeActiveControls, N'$."KeyName"')` and compare with `N'true'`.

Use only the documented keys listed in Section 8.5 and Section 15.3. Do not invent keys.

**Interpreting controls against tag status**

A tag status does not automatically prohibit every operation. Evaluate the status against the relevant control configured for the actual action type.

- For freeze-related movement restrictions, inspect the `Freezed` key. The control is active only when its JSON value is exactly the string `"true"`. Evaluate the tag's current `Freeze` value and the operation's applicable source and destination conditions.
- For `Lock = 1`, determine whether the inspection-failure blocking status affects the requested operation by checking the actual documented control. Do not invent a control key or assume that every operation type applies this restriction.
- For cancellation, inspect `AllowCancelTag` in the `DestinationEpc` JSON of the tag's current destination. Do not substitute `Lock`, `Freeze`, or action-type movement controls for this permission check.
- A control being enabled does not, by itself, prove that it caused an operation to be denied. Its documented meaning and applicability to the requested operation must also be established.
- Do not treat missing keys, NULL values, or unrecognized values as `"true"` or `"false"`.

### 8.5 Direct Queries About Active Operation Controls

When the user asks which controls are active for an operation, this is a direct database lookup request. It is not merely a knowledge-base question and does not require a denied-operation scenario.

Examples include:
- "کنترل‌های فعال عملیات ارسال کالا به فروش چیا هستن؟"
- "برای ارسال کالا به فروش چه کنترل‌هایی فعاله؟"
- "کنترل‌های فعال نوع عملیات [operation title] چیست؟"

**Mandatory workflow:**

1. Extract the operation's business title from the user's question.
2. Search `tbl_ActionTypes` using `fld_ActionTypeTitle`. Use the actual configured title; never invent an operation ID.
3. Read `fld_ActionTypeActiveControls` from the matching record.
4. Check every documented control key. A control is active only if its stored JSON value is exactly the string `"true"`.
5. Report only active controls. Explain their meanings using the documented Active Operation Controls knowledge-base section.
6. Answer in Persian using clear business language. Do not expose SQL, table names, column names, JSON keys, or internal IDs.
7. If the operation is found but its configuration is NULL or unavailable, explain that the operation was found but its active controls could not be verified.
8. If the operation exists and the configuration contains no documented control with the value `"true"`, report that no documented active controls were found in the retrieved configuration.
9. If the query returns no matching operation, do not immediately claim that the information is absent from the knowledge base. Check the operation-title search and report that the matching operation could not be found in the database only after that lookup fails.

**Important:** For direct questions about active controls, executing the database search is mandatory. Do not answer only from RAG documents, and do not tell the user to consult operation settings instead of retrieving them.

#### Search Command Template

Replace `[USER_OPERATION_TITLE]` with the operation title extracted from the user's message. This is a template, not a literal SQL parameter.

<<Search
SELECT
    act.fld_ActionTypeTitle AS [نوع عملیات],
    CASE
        WHEN act.fld_ActionTypeActiveControls IS NULL
            THEN N'تنظیمات کنترل‌ها ثبت نشده است'
        WHEN activeControls.ActiveControlNames IS NULL
            THEN N'هیچ کنترل مستندی با مقدار true یافت نشد'
        ELSE activeControls.ActiveControlNames
    END AS [کنترل‌های فعال]
FROM tbl_ActionTypes AS act
OUTER APPLY
(
    SELECT STRING_AGG(c.ControlName, N', ') AS ActiveControlNames
    FROM
    (
        VALUES
            (N'NotInSourceIgnore', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.NotInSourceIgnore')),
            (N'NotInSourceError', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.NotInSourceError')),
            (N'Freezed', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.Freezed')),
            (N'FailedQC', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.FailedQC')),
            (N'NotQc', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.NotQc')),
            (N'NotRegisterIgnore', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.NotRegisterIgnore')),
            (N'NotRegisterError', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.NotRegisterError')),
            (N'TagLifeLessThanIgnore', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.TagLifeLessThanIgnore')),
            (N'DocCodeSet', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.DocCodeSet')),
            (N'DocItemsCheck1', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.DocItemsCheck1')),
            (N'DocItemsCheck2', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.DocItemsCheck2')),
            (N'DocItemsCheck3', JSON_VALUE(act.fld_ActionTypeActiveControls, N'$.DocItemsCheck3'))
    ) AS c(ControlName, ControlValue)
    WHERE c.ControlValue = N'true'
) AS activeControls
WHERE act.fld_ActionTypeTitle LIKE N'%[USER_OPERATION_TITLE]%';
>>

The query must use the actual operation title supplied by the user. Do not leave `[USER_OPERATION_TITLE]` in the executed SQL. If the title search returns multiple operation types, do not combine their controls; identify the intended operation or ask the user to clarify.

The returned control names are internal identifiers. Convert them into user-facing Persian explanations using the documented control definitions. Never invent a meaning for a control whose meaning is not documented.

### 8.6 Active-Control Key Reference

Use only the documented keys listed in Section 8.5's search template. A control being active means that the operation has that configured control; it does not prove that a particular serial violates the control.

| Key | User-facing Persian name | Meaning |
|---|---|---|
| `NotInSourceIgnore` | نادیده گرفتن تگ خارج از منبع | A tag that is not at the expected source is ignored and does not stop the operation |
| `NotInSourceError` | خطا در صورت نبودن تگ در منبع | If the tag is not at the expected source, the operation stops with an error |
| `Freezed` | کنترل کالای فریز | The frozen status of the item is checked before the operation |
| `FailedQC` | کنترل کالای مردود در کنترل کیفیت | Items that failed quality control are restricted |
| `NotQc` | کنترل کالای بدون کنترل کیفیت | Items that do not yet have a quality control result are restricted |
| `NotRegisterIgnore` | نادیده گرفتن تگ ثبت‌نشده | An unregistered tag/serial is ignored and does not stop the operation |
| `NotRegisterError` | خطا در صورت ثبت‌نشده بودن تگ | An unregistered tag/serial cannot continue through the operation |
| `TagLifeLessThanIgnore` | نادیده گرفتن عمر کمتر از حد مجاز تگ | A tag whose age is below the required minimum is ignored |
| `DocCodeSet` | اجباری بودن شماره سند | The document number/information is mandatory for the operation |
| `DocItemsCheck1` | کنترل تطابق دقیق اقلام سند | The identified items must exactly match the items listed in the document. |
| `DocItemsCheck2` | کنترل سند عمومی | The product code must exist in the document, and the cumulative processed quantity must not exceed the total quantity specified in the document. |
| `DocItemsCheck3` | کنترل اقلام سند |

When one of the `DocItemsCheck` keys is true, explain that the operation applies document-items checks against the related document, without claiming which individual key corresponds to which check.

### 8.7 JSON Interpretation Rules

Before writing a query:

1. Determine which entity owns the requested property.
2. Identify the correct JSON column.
3. Determine the actual JSON property name and path from the data or application definition.
4. Use `JSON_VALUE` for scalar properties.
5. Do not assume that a JSON property exists in every record.
6. Do not invent a JSON property name or treat an unverified property as a confirmed schema field.

A property with a similar name may exist in multiple JSON columns. The correct source depends on the business meaning of the request.

### 8.8 Other JSON/Text-Structured Columns

The following columns also hold JSON or structured text. Their internal property names are not documented, so never invent a JSON key:

`tbl_Products.ProductProperties`, `tbl_Print.ProductProperties`, `tbl_ProductPropertyA/B/C.*Data`, `tbl_Station.fld_StationReaders`, `tbl_Station.fld_StationSettings`, `tbl_UHFReaderLogHeader.fld_CarProperties`, `tbl_TruckCross.fld_TruckCrossDynamicFields`, `tbl_DocumentHeader.fld_DocumentHeaderData`, `tbl_DocumentItem.fld_DocumentItemsData`, `tbl_Gallery.fld_GalleryAdditionalData`, `tbl_DynamicFields.fld_DynamicFieldValueOptions`.

The custom fields that exist in the system are defined in `tbl_DynamicFields` (`fld_DynamicFieldTitle`). Use a JSON key only when the user or the documentation provides its exact name.

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

- What is the total quantity?(مقدار کل چقدر است؟)
- مجموع موجودی
- مجموع مقدار کالا
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

### 9.5 Grouping by a Lookup Attribute

When the user asks for a total or count "by" a lookup attribute (for example inventory by brand, by size, by warehouse), join the lookup table and group by its title column (Section 4.5). Do not group by or display the code.

---

## 10. Date and Time Logic

Date filtering and date display are separate operations. They must not be confused.

### 10.1 Date Fields Have Different Storage Formats

The database contains different date representations, including:

- Gregorian `datetime` columns, such as `TagRegisterDateTime`, `MovementActionDateTime`, `fld_TagRead_DateTimeMiladi`, `PrintActionDateTime`, `fld_HeaderCreateDateTime`, `fld_DocumentImportDatetime`, and the datetime columns of `tbl_TruckCross`.
- Separate date and time fields, such as `MovementActionDate` and `MovementActionTime`.
- Shamsi date strings, such as `TagRegisterShamsiUnixDate`, `fld_TagRead_DateTime`, `RegisterDate`, `fld_ProductExpireStartDate`, `fld_ProductExpireEndDate`, `fld_ProductGuaranteeEndDate`, `fld_ProductExpireEndDate` of `tbl_Products`.
- Movement-specific date/time fields, such as `RTagsMovementDateTime` and `HTagsMovementDateTime`.

Do not assume all date fields use the same storage format or business meaning.

Before writing a date filter, identify the actual field and its storage format.

The calendar used by the short text date columns (`nvarchar(10)` such as `MovementActionDate`, `RTagsMovementDate`, `HTagsMovementDate`) is not documented. Prefer the `datetime` column of the same record for filtering and for display (for example `MovementActionDateTime`). Do not apply a conversion function to a text date column.

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

### 10.7 Filtering by the Shamsi Registration Timestamp

Because `tbl_Tags.TagRegisterShamsiUnixDate` has the fixed-width format `YYYYMMDDHHmmss`, a Shamsi date range can be filtered by string comparison on that format (for example `>= N'14020618000000' AND <= N'14020618235959'`). Do not apply any date function to this column.

---

## 11. Core Relationship Map

### 11.1 Product and Inventory

`tbl_Products.ProductCode`  
↔ `tbl_Tags.ProductCode`

This relates a master product definition to its registered inventory/tag records. The classification columns of `tbl_Products` and their lookup tables are in Section 4.3.

### 11.2 Inventory and Movement History

`tbl_Tags.ProductSerial`  
↔ `tbl_TagsMovement.ProductSerial`

The appropriate movement-action path is then:

- Entry/registration: `RMovementActionId`
- Exit/return: `HMovementActionId`

Both paths lead to `tbl_MovementActions.MovementActionId`. `tbl_TagsMovement.ProductCode` also relates to `tbl_Products.ProductCode`.

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

`tbl_Zones.ZoneCorridorId`  
→ `tbl_Corridor.fld_CorridorId`

`tbl_Corridor.fld_CorridorWarehouseCode`  
→ `tbl_Destination.DestinationCode`

### 11.6 Product Classification

The following code-based relationships are documented:

- `tbl_Tags.fld_ProductBrand` → `tbl_ProductBrand.fld_ProductBrandCode`
- `tbl_Tags.fld_ProductGroup` → `tbl_ProductGroup.fld_ProductGroupCode`
- `tbl_Tags.fld_ProductSubGroup` → `tbl_ProductSubGroup.fld_ProductSubGroupCode`
- `tbl_Tags.fld_ProductClass` → `tbl_ProductClass.fld_ProductClassCode`
- `tbl_Tags.ProductStatus` → `tbl_ProductStatus.ProductStatusCode`
- `tbl_Tags.ProductType` → `tbl_ProductType.ProductTypeCode`
- `tbl_Products.fld_ProductBrand`, `fld_ProductGroup`, `fld_ProductSubGroup`, `fld_ProductClass`, `ProductStatus`, `ProductType` → the same lookup code columns
- `tbl_ProductSubGroup.fld_ProductGroupCode` → `tbl_ProductGroup.fld_ProductGroupCode`

### 11.7 Product Properties

- `tbl_Tags.fld_ProductPropertyAId` → `tbl_ProductPropertyA.fld_ProductPropertyAId`
- `tbl_Tags.fld_ProductPropertyBId` → `tbl_ProductPropertyB.fld_ProductPropertyBId`
- `tbl_Tags.fld_ProductPropertyCId` → `tbl_ProductPropertyC.fld_ProductPropertyCId`
- `tbl_Products.ProductSize` → `tbl_ProductPropertyC.fld_ProductPropertyCId`
- `tbl_ProductPropertyB.fld_ProductPropertyAId` → `tbl_ProductPropertyA.fld_ProductPropertyAId`

### 11.8 Station and User

`tbl_MovementActions.MovementActionUHFLogGate`  
→ `tbl_Station.fld_StationCode`

`tbl_MovementActions.MovementActionUserId`  
→ `tbl_User.Id`

The station's operation-type configuration is a separate relationship:

`tbl_Station.fld_StationActionType`  
→ `tbl_ActionTypes.fld_ActionTypeId`

Other user references: `tbl_Tags.TagRegisterUser`, `tbl_Products.ProductRegUser`, `tbl_DocumentHeader.fld_DocumentSaveUserId`, `tbl_FreezeHeader.fld_FreezeUserId`, `tbl_Gallery.fld_GalleryUserId`, `tbl_ProductExpire.fld_ProductExpireLastModifiedUserId`, `tbl_DynamicFields.fld_DynamicFieldUser`, `tbl_NotificationOrders.fld_NOUserId`, `tbl_UHFReaderLogHeader.fld_UHFReaderLogHeaderUserId`, `tbl_Print.SoftDeleteUser` → `tbl_User.Id`.

### 11.9 RFID Logs

`tbl_Tags.TagEpc`  
↔ `tbl_UHF_ReaderLog.fld_TagSerial`

`tbl_UHF_ReaderLog.fld_ProductSerial` → `tbl_Tags.ProductSerial`

`tbl_UHF_ReaderLog.fld_InventoryId` → `tbl_UHFReaderLogHeader.fld_UHFReaderLogHeaderId`

`tbl_UHF_ReaderLog.MovementActionId` → `tbl_MovementActions.MovementActionId`

`tbl_UHFReaderLogHeader.fld_MovementActionId` → `tbl_MovementActions.MovementActionId`

`tbl_UHFReaderLogHeader.fld_StationCode` → `tbl_Station.fld_StationCode`

`tbl_UHFReaderLogHeader.fld_TruckCrossId` → `tbl_TruckCross.fld_TruckCrossId`

Use this group for RFID detection questions.

### 11.10 Truck Crossings

`tbl_MovementActions.MovementActionTruckCrossId`  
→ `tbl_TruckCross.fld_TruckCrossId`

The lookup relationships of `tbl_TruckCross` and its items are in Section 4.4. Also: `tbl_TruckCrossItem.TruckCrossId` → `tbl_TruckCross.fld_TruckCrossId`; `tbl_TruckCrossOperationType.TruckCrossCauseId` → `tbl_TruckCrossCause.fld_TruckCrossCauseId`; `tbl_TruckCrossCause.fld_TruckCrossCauseEnterActionTypeId` and `fld_TruckCrossCauseExitActionTypeId` → `tbl_ActionTypes.fld_ActionTypeId`; `tbl_TruckCrossShippingFee.CompanyId` → `tbl_TruckCompany.fld_TruckCompanyId`, `CustomerId` → `tbl_TruckCrossCustomer.fld_TruckCrossCustomerId`, `ProductTypeId` → `tbl_TruckCrossProductType.fld_TruckCrossProductTypeId`, `ShipmentId` → `tbl_TruckCrossShipment.fld_TruckCrossShipmentId`.

### 11.11 Placement Missions

`tbl_ProductPlacementMissions.fld_PPMProductSerial`  
→ `tbl_Tags.ProductSerial`

(The mission table's column definition is not available; see Section 2.)

### 11.12 Handheld Inventory

`tbl_InventoryTags.fld_InventoryTagEPC`  
→ `tbl_Tags.TagEpc`

Use this relationship for handheld inventory records, not as an automatic substitute for physical-counting workflows. (The table's column definition is not available; see Section 2.)

### 11.13 Documents

- `tbl_DocumentItem.fld_DocumentKey` → `tbl_DocumentHeader.fld_DocumentKey`
- `tbl_DocumentItem.fld_DocumentItemProductCode` → `tbl_Products.ProductCode`
- `tbl_DocumentHeader.fld_DocumentStatus` → `tbl_DocumentStatus.fld_DocumentStatusId`
- `tbl_DocumentHeader.fld_DocumentSaveUserId` → `tbl_User.Id`
- Document keys carried by other tables: `tbl_MovementActions.MovementActionDocumentId`, `tbl_TagsMovement.RMovementActionDocumentId`, `tbl_UHFReaderLogHeader.fld_DocumentCode`, `tbl_UHF_ReaderLog.fld_DocumentId`, `tbl_Print.DocumentId` → `tbl_DocumentHeader.fld_DocumentKey`

### 11.14 Freeze

- `tbl_FreezeItem.fld_FreezeHeaderId` → `tbl_FreezeHeader.fld_FreezeHeaderId`
- `tbl_FreezeItem.fld_FreezeProductSerial` → `tbl_Tags.ProductSerial`
- `tbl_FreezeHeader.fld_FreezeUserId` → `tbl_User.Id`

### 11.15 Print Log

- `tbl_Print.ProductCode` → `tbl_Products.ProductCode`
- `tbl_Print.ProductSerial` → `tbl_Tags.ProductSerial`
- `tbl_Print.DestinationCode` → `tbl_Destination.DestinationCode`

### 11.16 Other Relationships

- `tbl_DynamicFields.fld_DynamicFieldActionType` → `tbl_ActionTypes.fld_ActionTypeId`
- `tbl_DynamicFields.fld_DynamicFieldSectionId` → `tbl_DynamicFieldSections.fld_DfSectionId`
- `tbl_ProductExpire.fld_ProductExpireProductCode` → `tbl_Products.ProductCode`
- `tbl_Products.ProductGalleryId` → `tbl_Gallery.fld_GalleryId`
- `tbl_NotificationOrders.fld_NOEventType` → `tbl_NotificationEventTypes.Id`
- `tbl_NotificationQueue.fld_NotificationOrderId` → `tbl_NotificationOrders.Id`

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

### 12.7 Declared Lengths and Other Facts

- The table definitions available for some tables declare slightly different lengths for the same column (for example `nvarchar(50)` or `nvarchar(max)`). Do not rely on declared lengths in a query.
- `tbl_Products`: the table has an identity `Id`; `ProductCode` is the business key; `ProductSize` is `nvarchar(50)`.
- `tbl_TruckCross.fld_TruckCrossId` and `tbl_MovementActions.MovementActionTruckCrossId` are `bigint`.
- `tbl_UHF_ReaderLog.fld_TagRead_DateTimeMiladi` is `datetime`; `fld_TagRead_DateTime` is `nvarchar(50)`.
- `tbl_Destination.DestinationType` is `int`; `tbl_DestinationType.fld_DestinationTypeCode` is `nvarchar`.

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

15. **Denied-operation questions.** If the user asks why an operation is not allowed or fails, do not answer with possible reasons. Follow Section 15 to retrieve the real tag state and the operation type controls.

16. **Direct active-control questions:** If the user asks which controls are active for a named operation, execute the Active Operation Controls Search Command in Section 8.5. Do not treat this as a knowledge-base-only question.

17. **Explain retrieved controls:** After retrieving the actual active controls, map each returned control to its documented business meaning and explain only those controls in Persian. If the database lookup fails, state the specific lookup limitation rather than claiming that the information is missing from the knowledge base.

18. **Titles, not codes.** For every attribute of a tag or product that references a lookup table (type, quality, group, subgroup, brand, class, production line, shift, size, warehouse, zone, user), join the lookup table and return only its title/name column. Return a code or ID only when the user explicitly asks for it (Section 4.5).

19. **Use the correct columns of the starting table.** From `tbl_Products` use its own classification columns (`fld_ProductGroup`, `ProductType`, `ProductSize` and so on, Section 4.3); from `tbl_Tags` use Table 4.5.1. Never use column names that do not exist on the starting table.

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
- `Lock = true` blocks exit and cancellation of a tag/serial; `Freeze = true` blocks exit. `false` does not restrict.
- For a denied-operation question, retrieve the actual tag state and the operation type controls instead of listing possible reasons.
- Attributes of tags and products that reference other tables are always displayed as titles/names, never as codes or IDs, unless the user explicitly asks for the code.
- The size of a product is resolved through `tbl_ProductPropertyC`; the product type through `tbl_ProductType`; quality through `tbl_ProductStatus`; brand, group, subgroup and class through their own lookup tables.

**The objective is not merely to generate syntactically valid SQL. The objective is to generate SQL that accurately represents the Silo WMS database and its real business processes.**

## 15. Operation Controls and Denial Diagnosis

### Purpose

Determine why a user cannot perform an operation on a serial or tag by retrieving actual database data and interpreting it according to the documented business rules.

The agent must identify the minimum database information required to answer the user's question. Do not retrieve every operation control automatically when the tag's own status is sufficient to explain the problem.

### 15.1 Decision Rules

When the user asks why an operation on a serial or tag is not allowed, follow this decision order.

**Step 1 — Identify the target and requested operation**

- Extract the serial number or EPC from the user's message.
- Identify the intended operation, if provided, such as خروج, ابطال, or برگشت به انبار محصول.
- Match the operation to the relevant action type by its configured title and documented meaning. Never guess an action type ID.

**Step 2 — Retrieve the tag's current status**

Retrieve the target tag's actual `Lock` and `Freeze` values from `tbl_Tags`.

- `Lock = true`: The tag/serial is marked as blocked following a failed goods inspection. Determine whether this status affects the requested operation by checking the actual documented control governing inspection-failure blocking.
- `Freeze = true`: The tag/serial is currently frozen. Determine whether the requested movement is subject to freeze checking by inspecting the relevant action type's `Freezed` control and its applicable source/destination conditions.
- `Lock = false` or `Freeze = false`: The corresponding status is not active. Do not interpret a false value as a restriction.
- NULL, missing, or unrecognized values: Do not assume that the restriction is active or that the tag is unrestricted.

Do not conclude that an operation is prohibited solely because `Lock = true` or `Freeze = true`.

**Step 3 — Evaluate operation-type controls for movement operations**

When the requested operation is a movement:

1. Retrieve the actual action type by matching its configured title to the operation described by the user.
2. Read `fld_ActionTypeActiveControls`.
3. Extract the documented control keys using `JSON_VALUE`.
4. For a frozen tag, check the `Freezed` control and the applicable source/destination conditions.
5. For a tag blocked following a failed inspection, check the actual documented control governing inspection-failure blocking.
6. Determine whether the relevant control applies to the requested operation and whether the retrieved tag status satisfies the condition for restriction.
7. Do not invent control keys or report a control as the cause of denial unless the retrieved data and documented meaning establish that conclusion.

**Step 4 — Evaluate cancellation permission separately**

When the requested operation is cancellation (ابطال):

1. Retrieve the target tag's current destination.
2. Resolve the destination using the documented relationship to `tbl_Destination`.
3. Inspect the `DestinationEpc` JSON for that destination.
4. Extract and evaluate `AllowCancelTag`.
5. If `AllowCancelTag = true`, report that the destination permits cancellation, subject to any other documented restrictions that have actually been checked.
6. If `AllowCancelTag = false`, report that the current destination does not permit cancellation.
7. If the destination cannot be resolved or the permission is missing, NULL, or unrecognized, report that the available data does not establish whether cancellation is permitted.

Do not conclude that cancellation is prohibited solely because `Lock = true`. Do not conclude that cancellation is permitted solely because `Lock = false` or `Freeze = false`.

**Step 5 — Report only confirmed findings**

Explain the reason only when it is supported by the retrieved data and documented business rules. Do not list generic possible causes as confirmed reasons.

If the relevant control, destination, permission value, or required relationship cannot be established, state what information is missing. Never invent operation IDs, control keys, relationships, or reasons for denial.

### 15.2 Tag Status Query — Additional Interpretation Rule

The tag status query retrieves the actual `Lock` and `Freeze` values. These values must be interpreted according to the documented business rules and the requested operation.

Do not conclude that `Lock = true` or `Freeze = true` independently prohibits an operation unless the documented rules establish that restriction for the requested operation.

If the status alone does not establish the reason for denial, investigate the relevant operation controls and other documented conditions only as needed.

### 15.3 Active Operation Controls

When Step 3 is required, retrieve the relevant operation's controls from `tbl_ActionTypes`.

Match the operation using `fld_ActionTypeTitle` with an appropriate `LIKE` condition. Never invent `fld_ActionTypeId`.

Retrieve the actual tag status and the active-control values in the same query where practical.

Extract all documented control keys using `JSON_VALUE`:

- `NotInSourceIgnore`
- `NotInSourceError`
- `Freezed`
- `FailedQC`
- `NotQc`
- `NotRegisterIgnore`
- `NotRegisterError`
- `TagLifeLessThanIgnore`
- `DocCodeSet`
- `DocItemsCheck1`
- `DocItemsCheck2`
- `DocItemsCheck3`

Every output column must have a Persian alias.

Use `LEFT JOIN` or another suitable join that does not accidentally remove the tag when the requested action type is not found. Do not use an unrestricted `CROSS JOIN` that can return multiple unrelated action types.

### 15.4 Interpreting Active Controls

Interpret only controls whose returned value is exactly the string `"true"`. Use the user-facing names and meanings documented in Section 8.6.

- `NotInSourceIgnore`: Tags outside the expected source are ignored and do not stop the operation.
- `NotInSourceError`: The tag must be at the expected source; otherwise, the operation stops with an error.
- `Freezed`: The operation checks whether the item is frozen. Evaluate the tag's current `Freeze` value and the operation's applicable source/destination conditions.
- `FailedQC`: Items that failed quality control are restricted only when the documented control applies to the requested operation.
- `NotQc`: Items without a quality-control result are restricted only when the documented control applies to the requested operation.
- `NotRegisterIgnore`: Unregistered tags or serials are ignored.
- `NotRegisterError`: The tag or serial must be registered before the operation.
- `TagLifeLessThanIgnore`: Tags below the minimum required age are ignored.
- `DocCodeSet`: Entering the document number is mandatory.
- `DocItemsCheck1`: The identified items must exactly match the items listed in the document.
- `DocItemsCheck2`: The product code must exist in the document, and the cumulative processed quantity must not exceed the total quantity specified in the document.
- `DocItemsCheck3`:  

When an ignore control and an error control both exist, interpret them according to the actual returned configuration and documented application behavior. Do not assume undocumented precedence rules.

A control being active does not prove that a specific tag or serial violates that control. For example, `NotRegisterError = true` establishes that registration is required; it does not prove that the requested serial is unregistered.

**Lock and inspection-failure status**

`Lock = true` indicates that the tag or serial is marked as blocked following a failed goods inspection. Do not infer that `Lock = true` independently prohibits exit, cancellation, or every other operation. Report an operation restriction only when the actual documented inspection-failure control, its applicability to the requested operation, and the relevant tag status establish that restriction.

**Cancellation permission**

`AllowCancelTag` is a destination-level cancellation permission stored in the `DestinationEpc` JSON of `tbl_Destination`. When diagnosing cancellation permission, evaluate this setting against the tag's current destination. Do not infer cancellation permission from `Lock`, `Freeze`, or action-type movement controls.

For controls whose evaluation data is unavailable, explain the applicable requirement without claiming that the serial or tag has failed that requirement.

Do not report a control as the cause of a denied operation unless its actual value, documented meaning, and applicability to the requested operation are established.

### 15.5 Final Answer Rules

- Answer in Persian, using plain business language.
- Explain the proven reason for the denial first.
- If Freeze is true, explain that the serial is frozen and exit is not permitted.
- Explain active action-type requirements only when the action-type controls have been retrieved and are relevant to the question.
- Do not list generic or hypothetical reasons as if they were findings.
- If several reasons are independently supported by the returned data, explain all of them.
- If no blocking reason is established, say that the retrieved data did not identify the cause. Do not invent one.
- Never expose database table names, column names, JSON keys, SQL queries, internal IDs, or technical implementation details to the end user.
- Do not describe the query-generation process or the internal diagnostic steps to the end user.

### 15.6 Expected Behavior Examples

**Example A — Lock is true**

User: "می‌خوام سریال 10000019 رو خروج بزنم، اما اجازه نمی‌ده. چرا؟"

Returned data: `Lock = true`, `Freeze = false`.

Expected behavior: Explain that the serial is marked as blocked following a failed goods inspection. Do not claim that this status independently prohibits exit unless the documented inspection-failure control and its applicability to the requested operation establish that restriction. If necessary, retrieve the relevant operation controls.

**Example B — Neither status establishes the denial**

User: "می‌خوام سریال 10000019 رو خروج بزنم، اما اجازه نمی‌ده. چرا؟"

Returned data: `Lock = false`, `Freeze = false`.

Expected behavior: Treat blocking and freezing as unconfirmed causes. Investigate the relevant exit action type and other documented conditions only as needed. Do not guess the reason.

**Example C — Return to product warehouse**

User: "می‌خوام این تگ رو برگشت به انبار محصول بزنم، ولی اجازه نمی‌ده."

Expected behavior: Check the tag status first. If the status does not establish the reason for denial, retrieve the action type for برگشت به انبار محصول and inspect its relevant active controls.

If `NotInSourceError = true` and `NotRegisterError = true`, explain that the tag must be at the expected source and must be registered. Do not claim that either condition has failed unless the retrieved data proves it.

**Core principle:** Retrieve the minimum necessary data first, expand the investigation only when required, and report verified findings rather than possible causes.

---

## 16. DATABASE SCHEMA (authoritative)

Legend: nv = nvarchar, nvmax = nvarchar(max), dec = decimal, NN = NOT NULL, PK = primary key, [..] = note. Declared lengths in the available sources differ slightly for some columns; do not rely on lengths.

tbl_Tags (products currently in the warehouse; one row = one tracked unit; PK = (ProductSerial, TagEpc)): Id int IDENTITY, ProductSerial nv(50) NN, ProductCode nv(50), TagEpc nv(50) NN, TagEpc2 nv(128), ProjectCode nv(50), ProductCount dec(18,2), ProductName nvmax, ProductType nv(256), ProductStatus nv(256), TagStatus nv(50), TagRegisterShamsiUnixDate nv(50) [Shamsi text, format YYYYMMDDHHmmss], TagRegisterDateTime datetime [Gregorian], TagRegisterUser nv(50), TagTreeParentId int, TagTreeSecondParentId int, TagTreeParentsId nvmax, TagTreeParentSerial nv(50), TagTreeParentsEpc nv(128), NewProductSerial nv(50), ProductProperties nvmax [JSON], Lock bit [true = blocked: no exit, no cancel], Username nv(50), DeviceId nv(50), DeviceIp nv(50), Freeze bit [true = frozen: no exit], Deactivate bit, TagInActionId int, TagInDestinationId nv(50) [warehouse code], TagInActionId2 int, TagInDestinationId2 nv(50), fld_ProductPropertyAId nv(50), fld_ProductPropertyBId nv(50), fld_ProductPropertyCId nv(50), RegCode nv(50), fld_LastModifierUser nv(128), ContractStatus nv(50), TagZone nv(50) [zone code], ReProduct bit, fld_InspectActionId int, fld_LastInspectResult nvmax, fld_ProductGroup nv(128), fld_ProductBrand nv(128), fld_ProductSubGroup nv(128), fld_ProductClass nv(128)

tbl_Products (master product / SKU; business key ProductCode): Id int IDENTITY, ProductCode nv(50), ProductTitle nv(250), ProductENTitle nv(250), ProductPackValue dec, ProductPackWeight dec, ProductPackVolume dec, ProductCountInPack dec, ProductValue dec, ProductTechnicalCode nv(50), ProductProperties nvmax [JSON], ProductType nv(50), ProductStatus nv(50), ProductSize nv(50), ProductUnit nv(50), ProductRegUser nv(50), ProductRegDateTime datetime, ProductGalleryId int, ProductTechnicalData nvmax [JSON], fld_ProductGroup nv(128), fld_ProductBrand nv(128), fld_ProductSubGroup nv(128), fld_ProductClass nv(128), fld_ProductGuaranteeType int, fld_ProductGuaranteeDays int, fld_ProductGuaranteeMonths int, fld_ProductGuaranteeEndDate nv(10) [Shamsi text], fld_ProductExpireType int, fld_ProductExpireDays int, fld_ProductExpireMonths int, fld_ProductExpireEndDate nv(10) [Shamsi text], fld_ProductIsActive bit, fld_HasDoubleTag bit

tbl_TagsMovement (line-level movement log): TagsMovementId int IDENTITY, TagEpc nv(50), ProductCode nv(50), ProductSerial nv(50), ProductCount dec(18,2), HMovementActionId int [exit/return path], HTagsMovementDate nv(10), HTagsMovementTime nv(5), HTagsMovementDateTime datetime, HTagsMovementSt int, RMovementActionId int [entry/registration path], RTagsMovementDate nv(10), RTagsMovementTime nv(5), RTagsMovementDateTime datetime, RTagsMovementSt int, MovementData nvmax, ApiSendStatus int, ApiSendUser nv(128), ApiSendDateTime datetime, ApiSendData nvmax, RMovementActionType int, RMovementActionDocumentId nv(512), RMovementActionUHFLogId nv(50), RMovementActionDestinationId nv(50)

tbl_MovementActions (operation header): MovementActionId int, MovementActionTp int, MovementActionStore nv(50), MovementActionUserId nv(128), MovementActionDate nv(10), MovementActionTime nv(5), MovementActionDateTime datetime, MovementActionCountTags int, MovementActionDestinationId nv(50), MovementActionCarPlaque nv(16), MovementActionDriverName nv(50), MovementActionDriverMobile nv(50) [personal data], MovementActionData nvmax [JSON], MovementActionUHFLogId nv(50), MovementActionLinkId int, MovementActionLinkDestId nv(50), MovementActionTruckCrossId bigint, MovementActionDesc nvmax, MovementActionDocumentId nv(512), MovementActionUHFLogGate nv(30)

tbl_ActionTypes (operation types): Id int IDENTITY, fld_ActionTypeId int, fld_ActionTypeFromDestinationType nvmax, fld_ActionTypeToTypeDestinationType nvmax, fld_ActionTypeTitle nvmax, fld_ActionTypePermitedDocStatus nvmax, fld_ActionTypeChangeDocStatus nvmax, fld_ActionTypeActiveControls nvmax [JSON, values are strings "true"/"false"], fld_ActionTypeRfidPower int, fld_ActionTypeChangeTagLocation int, fld_ActionTypeProductType nvarchar

tbl_Station: fld_StationId int IDENTITY, fld_StationCode nv(128), fld_StationName nv(512), fld_StationType int, fld_StationActionType int, fld_StationStatus int, fld_StationReaders nvmax, fld_StationDescription nv(1024), fld_StationSettings nvmax, fld_StationFromDestination nvmax, fld_StationToDestination nvmax, fld_StationMacAddress nv(50)

tbl_Destination (warehouses): DestinationId int IDENTITY, DestinationTitle nv(50), DestinationSt int, DestinationDesc nvmax, DestinationCode nv(50) [business key], DestinationType int, DestinationParentId int, DestinationParentsId nvmax, DestinationEpc nvmax
tbl_DestinationType: fld_Id int IDENTITY, fld_DestinationTypeCode nv(50), fld_DestinationTypeName nv(250)
tbl_Zones (locations of a warehouse): id int IDENTITY, ZoneCode nv(50), ZoneTitle nv(50), ZoneCapacity dec(18,2), ZoneDimention nv(50), ZoneParentCode nv(50), ZoneParentLayer int, ZoneStoreCode nv(50) [warehouse code], ZoneCountPixle int, ZoneOccupiedCapacity int, MinZoneCapacity dec(18,2), MaxZoneCapacity dec(18,2), ZoneRowIndex int, ZoneType int, ZoneAddress nv(250), ZoneCorridorId int, ZoneMapXPosition int, ZoneMapYPosition int
tbl_Corridor: fld_CorridorId int IDENTITY, fld_CorridorWarehouseCode nv(50) NN, fld_CorridorName nv(128) NN, fld_CorridorDirection int, fld_CorridorVerticalOrder int, fld_CorridorHorizontalOrder int, fld_CorridorWidth int, fld_CorridorZoom int, fld_CorridorIsFaken bit NN

tbl_User (only these columns may be used): Id nv(128) PK, Name nv(512) NN, Username nv(256) NN, IsActive bit, CreateDate datetime, LastModifiedDate datetime

tbl_ProductBrand: fld_ProductBrandId int IDENTITY PK, fld_ProductBrandCode nv(128) NN, fld_ProductBrandTitle nv(128) NN, fld_ProductBrandData nvmax
tbl_ProductGroup: fld_ProductGroupId int IDENTITY PK, fld_ProductGroupCode nv(128) NN, fld_ProductGroupTitle nv(128) NN, fld_ProductGroupData nvmax
tbl_ProductSubGroup: fld_ProductSubGroupId int IDENTITY, fld_ProductSubGroupCode nv(128) NN, fld_ProductSubGroupTitle nv(256) NN, fld_ProductSubGroupSubTitle nv(512), fld_ProductGroupCode nv(128) NN, fld_ProductSubGroupDesc nv(512)
tbl_ProductClass: fld_ProductClassId int IDENTITY, fld_ProductClassCode nv(128) NN, fld_ProductClassTitle nv(256) NN, fld_ProductClassSubTitle nv(512), fld_ProductClassDesc nv(512)
tbl_ProductStatus (quality): ProductStatusId int IDENTITY PK, ProductStatusTitle nv, ProductStatusCode nv, ProductStatusDesc nvmax
tbl_ProductType (type tree): ProductTypeId int IDENTITY PK, ProductTypeTitle nv(50), ProductTypeParentId nv, ProductTypeParentsId nvmax, ProductTypeCode nv(50)
tbl_ProductPropertyA (production lines): fld_ProductPropertyAId nv(128) NN, fld_ProductPropertyATitle nv(256) NN UNIQUE, fld_ProductPropertyADesc nvmax, fld_ProductPropertyAData nvmax NN
tbl_ProductPropertyB (work shifts; hours in Desc, e.g. 06:00-14:00): fld_ProductPropertyBId nv(128) NN, fld_ProductPropertyBTitle nv(512) NN, fld_ProductPropertyBDesc nvmax, fld_ProductPropertyBData nvmax NN, fld_ProductPropertyAId nv(128)
tbl_ProductPropertyC (sizes): fld_ProductPropertyCId nv(128) NN, fld_ProductPropertyCTitle nv(256) NN, fld_ProductPropertyCDesc nvmax, fld_ProductPropertyCData nvmax NN, fld_ProductPropertyCIdentity int IDENTITY, fld_ProductPropertyCTemp nvmax

tbl_UHFReaderLogHeader: fld_UHFReaderLogHeaderId int IDENTITY, fld_StationCode nv(128), fld_ActionType nv(128), fld_DocumentCode nv(450), fld_TruckCrossId bigint, fld_UHFReaderLogHeaderUserId nv(128), fld_UHFReaderLogHeaderControlType int, fld_CarProperties nvmax [JSON], fld_HeaderUsedStatus int, fld_MovementActionId int, fld_HeaderCreateDateTime datetime
tbl_UHF_ReaderLog (raw RFID reads; filter dates with fld_TagRead_DateTimeMiladi): id int IDENTITY, fld_TagSerial nv(50) [tag EPC read], fld_Reader_Gate nv(256), fld_ReaderIp nv(15), fld_TagRead_DateTime nv(50) [Persian date text], fld_TagSelectedFlag tinyint, fld_InventoryId int, fld_Reader_GateType int, fld_Desc nv(256), fld_ReaderDeviceType int, ActionStatus int, ActionDesc nv(250), fld_DocumentId nv(128), fld_WMUserId nv(50), fld_InventoryPackage int, MovementActionId int, fld_TagRead_DateTimeMiladi datetime [Gregorian], fld_SaveUserId nv(128), fld_ProductSerial nv(50)

tbl_DocumentHeader: fld_Id int IDENTITY, fld_DocumentKey nv(450) NN [business key], fld_DocumentSaveUserId nv(50), fld_DocumentImportType int NN [0 Excel, 1 Api, 2 Manual, 3 Other, 4 Aggregate, 5 Divide], fld_DocumentImportFileName nvmax, fld_DocumentType nvmax, fld_DocumentType1 nvmax, fld_DocumentType2 nvmax, fld_DocumentImportDatetime datetime, fld_DocumentDesc nv(200), fld_DocumentStatus int NN, fld_DocumentHeaderData nvmax [JSON], fld_DocumentParent nvmax, fld_DocumentAggStatus int, fld_DocumentDivideParent nvmax, fld_DocumentChangeStatusLastUserId nv(50), fld_DocumentCheckType int
tbl_DocumentItem: fld_Id int IDENTITY, fld_DocumentKey nv(450), fld_DocumentType nvmax NN, fld_DocumentType1 nvmax NN, fld_DocumentType2 nvmax NN, fld_DocumentItemProductCode nv(50), fld_DocumentItemProductTitle nv(50), fld_DocumentItemCount dec(18,2) NN [sum it for document totals], fld_DocumentItemProducUnit nv(50), fld_DocumentItemsData nvmax [JSON]
tbl_DocumentStatus: fld_DocumentStatusId int NN, fld_DocumentStatusTitle nv(256) NN, fld_DocumentStatusIsUpdatePermitted bit NN, fld_DocumentStatusIsCartablePermitted bit NN

tbl_FreezeHeader: fld_FreezeHeaderId int IDENTITY, fld_FreezeUserId nv(128), fld_FreezeSaveDateTime datetime, fld_FreezeDesc nv(256), fld_FreezeResult bit NN
tbl_FreezeItem: fld_FreezeItemId int IDENTITY, fld_FreezeProductSerial nv(50), fld_FreezeHeaderId int NN

tbl_Print (tag print/registration log; SoftDelete = 1 means removed): Id int IDENTITY, ProductSerial nv(50), ProductCode nv(50), ProductName nv(500), ProductDescription nv(300), ProductType nv(50), ProductCount dec(18,2), ProductItemCount dec(18,2), ProductCountInPack dec(18,2), ProductUnit nv(50), ProductSize nv(50), ProductRegCode nv(50), ProductWeight dec(18,2), ProductVolume dec(18,2), ProductStatus nv(50), ProjectCode nv(50), TagEpc nv(50), ProductProductionShift nv(50), ProductProductionLine nv(50), ProductContractType int, PackageId int, Location nv(50), PrintActionId int, PrintType nv(50), PrintActionDateTime datetime, PrintUser nv(50), PrintFlag int, RegisterActionDateTime datetime, RegisterFlag int, RegisterType int, InputFileName nv(50), ErrorTime nv(50), ErrorDesc nv(200), SoftDelete int, ReRegister bit, PrintQueue bit, DocumentId nv(50), DocumentItemId nv(50), offline_epc nv(50), offline_reg_status int, offline_regDate nv(10), RegisterUser nv(50), RegisterDate nv(50), Manufacturer nv(50), AntiFire nv(50), DestinationCode nv(50), SoftDeleteUser nv(50), SoftDeleteDate nv(50), fld_ProductGroup nv(128), fld_ProductBrand nv(128), ProductProperties nvmax [JSON], fld_ProductSubGroup nv(128), fld_ProductClass nv(128)

tbl_TruckCross: fld_TruckCrossId bigint IDENTITY, fld_TruckCrossPlaque nv(20), fld_TruckCrossInternationalPlaque nv(20), fld_TruckCrossDriverName nv(50), fld_TruckCrossDriverPhone nv(20) [personal], fld_TruckCrossNationalCode nv(20) [personal], fld_TruckCrossPassportCode nv(50) [personal], fld_TruckCrossSerial nvmax, fld_TruckCrossCompany int, fld_TruckCrossType int, fld_TruckCrossTypeDesc nv(256), fld_TruckCrossLicenseCode nv(50), fld_TruckCrossStatus int, fld_TruckCrossPresentCause int, fld_TruckCrossPresentTurn int, fld_TruckCrossPresentDateTime datetime, fld_TruckCrossPresentDesc nv(250), fld_TruckCrossPresentUserId nv(128), fld_TruckCrossPresentOperationType int, fld_TruckCrossPresentOperationDestination int, fld_TruckCrossPresentShipment int, fld_TruckCrossPresentShipmentNumber nv(50), fld_TruckCrossPresentCustomer int, fld_TruckCrossEnterDateTime datetime, fld_TruckCrossEnterWeightTonage dec, fld_TruckCrossEnterDesc nv(250), fld_TruckCrossEnterUserId nv(128), fld_TruckCrossEnterEpc nv(50), fld_TruckCrossEnterOtherEpcs nv(250), fld_TruckCrossEnterAcceptor nv(50), fld_TruckCrossEnterAcceptPlace int, fld_TruckCrossPresentRevokeDateTime datetime, fld_TruckCrossPresentRevokeUserId nv(128), fld_TruckCrossExitDateTime datetime, fld_TruckCrossExitDesc nv(250), fld_TruckCrossExitUserId nv(128), fld_TruckCrossExitWeightTonage dec, fld_TruckCrossExitGateId int, fld_TruckCrossExitDestination nv(128), fld_TruckCrossExitPureWeightCargo dec, fld_TruckCrossExitWeightbridgeReceiptNumber nv(50), fld_TruckCrossExitCargoOwnerName nv(50), fld_TruckCrossExitCargoOwnerPhone nv(50) [personal], fld_TruckCrossExitDeliveryAddress nv(250), fld_TruckCrossExitShipmentCost nv(50), fld_TruckCrossExitPaymentType int, fld_TruckCrossExitUnitPrice nv(50), fld_TruckCrossExitTotalCost nv(50), fld_TruckCrossExitDistance nv(50), fld_TruckCrossDynamicFields nvmax [JSON]
tbl_TruckCrossItem: fld_TruckCrossItemId int IDENTITY, fld_TruckCrossItemTitle nv(256), Type int, ProductUnit nv(50), ProductCount dec, ProductSerial nv(50), ProductCode nv(50), TruckCrossProductTypeId int, TruckCrossId bigint
tbl_TruckCompany: fld_TruckCompanyId int IDENTITY, fld_TruckCompanyTitle nv(256)
tbl_TruckType: fld_TruckTypeId int IDENTITY, fld_TruckTypeTitle nv(256)
tbl_TruckCrossCause: fld_TruckCrossCauseId int IDENTITY, fld_TruckCrossCauseTitle nv(256), fld_TruckCrossCauseEnterActionTypeId int, fld_TruckCrossCauseExitActionTypeId int
tbl_TruckCrossOperationType: fld_TruckCrossOperationTypeId int IDENTITY, fld_TruckCrossOperationTypeTitle nv(256), TruckCrossCauseId int
tbl_TruckCrossOperationDestination: fld_TruckCrossOperationDestinationId int IDENTITY, fld_TruckCrossOperationDestinationTitle nv(256)
tbl_TruckCrossShipment: fld_TruckCrossShipmentId int IDENTITY, fld_TruckCrossShipmentTitle nv(256)
tbl_TruckCrossCustomer: fld_TruckCrossCustomerId int IDENTITY, fld_TruckCrossCustomerTitle nv(256)
tbl_TruckCrossAcceptPlace: fld_TruckCrossAcceptPlaceId int IDENTITY, fld_TruckCrossAcceptPlaceTitle nv(256)
tbl_TruckCrossProductType: fld_TruckCrossProductTypeId int IDENTITY, fld_TruckCrossProductTypeTitle nv(256), TruckCrossCauseIdsArray nvmax
tbl_TruckCrossShippingFee: fld_TruckCrossShippingFeeId int IDENTITY, CompanyId int, CustomerId int, ProductTypeId int, ShipmentId int, FromDate nv(50), ToDate nv(50), FeeStatus bit, FeeAmount dec, FeeWeight dec, FeeDistance dec

tbl_DynamicFields: fld_DynamicFieldId int IDENTITY, fld_DynamicFieldUser nv(128), fld_DynamicFieldTitle nv(50), fld_DynamicFieldType int NN, fld_IsSystematicField bit NN, fld_IsHeaderKey bit NN, fld_DynamicFieldDateTime datetime, fld_DynamicFieldRelatedTitle1 nv(50), fld_DynamicFieldRelatedTitle2 nv(50), fld_DynamicFieldRelatedTitle3 nv(50), fld_DynamicFieldActionType int, fld_DynamicFieldShowColumn bit NN, fld_DynamicFieldShowColumnForAction bit NN, fld_DynamicFieldDocGroupAggregate bit NN, fld_DynamicFieldValueType int, fld_DynamicFieldDefaultValue nv(128), fld_DynamicFieldValueOptions nvmax, fld_DynamicFieldRequirement bit, fld_DynamicFieldOrder int, fld_DynamicFieldSectionId int, fld_DynamicFieldParentId int, fld_DynamicFieldIsReadOnly bit
tbl_DynamicFieldSections: fld_DfSectionId int NN, fld_DfSectionTitle nv(256) NN, fld_DfType int NN
tbl_ProductExpire: fld_ProductExpireId int IDENTITY, fld_ProductExpireProductSerial nv(50), fld_ProductExpireProductCode nv(50), fld_ProductExpireStatus int, fld_ProductExpireStartDate nv(10) [Persian text], fld_ProductExpireEndDate nv(10) [Persian text], fld_ProductExpireActivationType int, fld_ProductExpireLastModifiedDateTime datetime, fld_ProductExpireLastModifiedUserId nv(128)
tbl_Gallery: fld_GalleryId int IDENTITY, fld_GalleryUserId nv(128), fld_GalleryMediaName nv(128), fld_GalleryMediaPath nv(512), fld_GalleryUsageType int, fld_GalleryUploadDateTime datetime, fld_GalleryUsageId nv(128), fld_GalleryMediaExtensionType int, fld_GalleryAdditionalData nvmax
tbl_NotificationEventTypes: Id int IDENTITY, fld_NETitle nv(50), fld_NECommand nvmax
tbl_NotificationOrders: Id int IDENTITY, fld_NOId int, fld_NOStatus int, fld_NODateTime datetime, fld_NOUserId nv(50), fld_NOType int, fld_NOTitle nv(50), fld_NOEventType int, fld_NOTimePeriod int, fld_NOSendDay nv(50), fld_NOSendClock nv(5), fld_NOSendType nv(50), fld_NOSendContacts nvmax, fld_NOContent nvmax
tbl_NotificationQueue: fld_Id int IDENTITY, fld_Text nvmax, fld_SendType int, fld_Contact nv(256), fld_SendDateTime datetime, fld_SendDate nv(10), fld_SendTime nv(5), fld_SendStatus int, fld_NotificationOrderId int, fld_QueueActionCode nv(256), fld_SaveDateTime datetime
tbl_Province: fld_Id int IDENTITY, fld_Name nv(50) NN

---

## 17. RELATIONSHIP CATALOG

These are logical business-key relationships (not necessarily enforced foreign keys). Use only these joins.

Product, classification and inventory:
- tbl_Tags.ProductCode → tbl_Products.ProductCode
- tbl_Tags / tbl_Products . fld_ProductBrand → tbl_ProductBrand.fld_ProductBrandCode
- tbl_Tags / tbl_Products . fld_ProductGroup → tbl_ProductGroup.fld_ProductGroupCode
- tbl_Tags / tbl_Products . fld_ProductSubGroup → tbl_ProductSubGroup.fld_ProductSubGroupCode
- tbl_Tags / tbl_Products . fld_ProductClass → tbl_ProductClass.fld_ProductClassCode
- tbl_Tags / tbl_Products . ProductStatus → tbl_ProductStatus.ProductStatusCode
- tbl_Tags / tbl_Products . ProductType → tbl_ProductType.ProductTypeCode
- tbl_Tags.fld_ProductPropertyAId → tbl_ProductPropertyA.fld_ProductPropertyAId; fld_ProductPropertyBId → tbl_ProductPropertyB.fld_ProductPropertyBId; fld_ProductPropertyCId → tbl_ProductPropertyC.fld_ProductPropertyCId
- tbl_Products.ProductSize → tbl_ProductPropertyC.fld_ProductPropertyCId
- tbl_Products.ProductRegUser → tbl_User.Id; tbl_Products.ProductGalleryId → tbl_Gallery.fld_GalleryId
- tbl_Tags.TagRegisterUser → tbl_User.Id
- tbl_ProductSubGroup.fld_ProductGroupCode → tbl_ProductGroup.fld_ProductGroupCode
- tbl_ProductPropertyB.fld_ProductPropertyAId → tbl_ProductPropertyA.fld_ProductPropertyAId

Location:
- tbl_Tags.TagInDestinationId → tbl_Destination.DestinationCode; tbl_Tags.TagZone → tbl_Zones.ZoneCode
- tbl_Zones.ZoneStoreCode → tbl_Destination.DestinationCode; tbl_Zones.ZoneCorridorId → tbl_Corridor.fld_CorridorId; tbl_Corridor.fld_CorridorWarehouseCode → tbl_Destination.DestinationCode
- tbl_Destination.DestinationType → tbl_DestinationType.fld_DestinationTypeCode

Movement:
- tbl_TagsMovement.ProductSerial → tbl_Tags.ProductSerial; tbl_TagsMovement.ProductCode → tbl_Products.ProductCode
- tbl_TagsMovement.RMovementActionId → tbl_MovementActions.MovementActionId (entry/registration path)
- tbl_TagsMovement.HMovementActionId → tbl_MovementActions.MovementActionId (exit/return path)
- tbl_MovementActions.MovementActionTp → tbl_ActionTypes.fld_ActionTypeId
- tbl_MovementActions.MovementActionUserId → tbl_User.Id
- tbl_MovementActions.MovementActionStore / MovementActionDestinationId → tbl_Destination.DestinationCode
- tbl_MovementActions.MovementActionUHFLogGate → tbl_Station.fld_StationCode
- tbl_MovementActions.MovementActionTruckCrossId → tbl_TruckCross.fld_TruckCrossId
- tbl_MovementActions.MovementActionDocumentId, tbl_TagsMovement.RMovementActionDocumentId → tbl_DocumentHeader.fld_DocumentKey
- tbl_Station.fld_StationActionType → tbl_ActionTypes.fld_ActionTypeId

RFID logs:
- tbl_UHF_ReaderLog.fld_TagSerial ↔ tbl_Tags.TagEpc; tbl_UHF_ReaderLog.fld_ProductSerial → tbl_Tags.ProductSerial
- tbl_UHF_ReaderLog.fld_InventoryId → tbl_UHFReaderLogHeader.fld_UHFReaderLogHeaderId; tbl_UHF_ReaderLog.MovementActionId → tbl_MovementActions.MovementActionId; tbl_UHF_ReaderLog.fld_DocumentId → tbl_DocumentHeader.fld_DocumentKey
- tbl_UHFReaderLogHeader.fld_StationCode → tbl_Station.fld_StationCode; fld_DocumentCode → tbl_DocumentHeader.fld_DocumentKey; fld_TruckCrossId → tbl_TruckCross.fld_TruckCrossId; fld_UHFReaderLogHeaderUserId → tbl_User.Id; fld_MovementActionId → tbl_MovementActions.MovementActionId

Documents, freeze, print:
- tbl_DocumentItem.fld_DocumentKey → tbl_DocumentHeader.fld_DocumentKey; tbl_DocumentItem.fld_DocumentItemProductCode → tbl_Products.ProductCode
- tbl_DocumentHeader.fld_DocumentStatus → tbl_DocumentStatus.fld_DocumentStatusId; tbl_DocumentHeader.fld_DocumentSaveUserId → tbl_User.Id
- tbl_FreezeItem.fld_FreezeHeaderId → tbl_FreezeHeader.fld_FreezeHeaderId; tbl_FreezeItem.fld_FreezeProductSerial → tbl_Tags.ProductSerial; tbl_FreezeHeader.fld_FreezeUserId → tbl_User.Id
- tbl_Print.ProductCode → tbl_Products.ProductCode; tbl_Print.ProductSerial → tbl_Tags.ProductSerial; tbl_Print.DocumentId → tbl_DocumentHeader.fld_DocumentKey; tbl_Print.DestinationCode → tbl_Destination.DestinationCode; tbl_Print.SoftDeleteUser → tbl_User.Id

Truck crossing:
- tbl_TruckCross.fld_TruckCrossCompany → tbl_TruckCompany.fld_TruckCompanyId; fld_TruckCrossType → tbl_TruckType.fld_TruckTypeId; fld_TruckCrossPresentCause → tbl_TruckCrossCause.fld_TruckCrossCauseId; fld_TruckCrossPresentOperationType → tbl_TruckCrossOperationType.fld_TruckCrossOperationTypeId; fld_TruckCrossPresentOperationDestination → tbl_TruckCrossOperationDestination.fld_TruckCrossOperationDestinationId; fld_TruckCrossPresentShipment → tbl_TruckCrossShipment.fld_TruckCrossShipmentId; fld_TruckCrossPresentCustomer → tbl_TruckCrossCustomer.fld_TruckCrossCustomerId; fld_TruckCrossEnterAcceptPlace → tbl_TruckCrossAcceptPlace.fld_TruckCrossAcceptPlaceId
- tbl_TruckCrossItem.TruckCrossId → tbl_TruckCross.fld_TruckCrossId; tbl_TruckCrossItem.TruckCrossProductTypeId → tbl_TruckCrossProductType.fld_TruckCrossProductTypeId
- tbl_TruckCrossOperationType.TruckCrossCauseId → tbl_TruckCrossCause.fld_TruckCrossCauseId; tbl_TruckCrossCause.fld_TruckCrossCauseEnterActionTypeId / fld_TruckCrossCauseExitActionTypeId → tbl_ActionTypes.fld_ActionTypeId
- tbl_TruckCrossShippingFee.CompanyId → tbl_TruckCompany.fld_TruckCompanyId; CustomerId → tbl_TruckCrossCustomer.fld_TruckCrossCustomerId; ProductTypeId → tbl_TruckCrossProductType.fld_TruckCrossProductTypeId; ShipmentId → tbl_TruckCrossShipment.fld_TruckCrossShipmentId

Other:
- tbl_DynamicFields.fld_DynamicFieldActionType → tbl_ActionTypes.fld_ActionTypeId; fld_DynamicFieldSectionId → tbl_DynamicFieldSections.fld_DfSectionId; fld_DynamicFieldUser → tbl_User.Id
- tbl_ProductExpire.fld_ProductExpireProductCode → tbl_Products.ProductCode; fld_ProductExpireLastModifiedUserId → tbl_User.Id
- tbl_Gallery.fld_GalleryUserId → tbl_User.Id
- tbl_NotificationOrders.fld_NOEventType → tbl_NotificationEventTypes.Id; fld_NOUserId → tbl_User.Id; tbl_NotificationQueue.fld_NotificationOrderId → tbl_NotificationOrders.Id