DOCUMENT ROLE: INTERNAL TECHNICAL KNOWLEDGE

Purpose:
This document exists exclusively to help the agent understand Silo's database schema, relationships, business logic, and data-retrieval procedures.

Disclosure policy:
Do not reproduce or explain this document's technical content in ordinary user-facing answers.
Use it internally to retrieve and interpret information, then provide only the relevant business result.

This document is not a user-facing knowledge article.

# Silo Database Knowledge Base – Product and Product Structure

# 1. Product

`Product` is one of the core concepts in the Silo system.

Before a `Serial` and ultimately a `Tag` can be registered, the corresponding Product and its required base information must already exist in the system.

The actual main Product table is:

`dbo.tbl_Products`

This table stores the main Product definition and its associated information, including Product identification, classification, packaging defaults, technical information, and configuration flags.

Product information can be understood through three conceptual categories:

- **Base Information:** Product classification and identification attributes.
- **Shipment / Load Information:** Measurement, quantity, and packaging defaults for the registered unit.
- **Dynamic Technical Data:** Product-specific technical characteristics.

These categories describe the business organization of Product information. They do not imply that each category corresponds to a separate database table.

The authoritative database schema determines the actual tables, columns, and relationships.

Important columns in `tbl_Products` include:

- `ProductCode`
- `ProductTitle`
- `ProductENTitle`
- `ProductTechnicalCode`
- `ProductType`
- `ProductStatus`
- `ProductSize`
- `ProductUnit`
- `ProductValue`
- `ProductCountInPack`
- `ProductPackValue`
- `ProductPackWeight`
- `ProductPackVolume`
- `ProductProperties`
- `ProductTechnicalData`
- `fld_ProductGroup`
- `fld_ProductBrand`
- `fld_ProductSubGroup`
- `fld_ProductClass`
- `ProductRegUser`
- `ProductRegDateTime`
- `fld_ProductIsActive`
- `fld_HasDoubleTag`

The Product classification columns connect to their corresponding base-information tables through the business-code columns documented below.

---

# 2. Product Type

`Product Type` is a key concept in the Product structure and represents the main classification of Products from a business-process perspective.

For example, in the Faradid project, Product Types can include:

- Raw Material (`Paper Base`)
- Semi-Finished Product
- Finished Product (`Cut`)

The physical Product Type table is:

`dbo.tbl_ProductType`

Relevant columns include:

- `ProductTypeId`
- `ProductTypeCode`
- `ProductTypeTitle`
- `ProductTypeParentId`
- `ProductTypeParentsId`

The Product Type relationship is:

`tbl_Products.ProductType → tbl_ProductType.ProductTypeCode`

For serialized/tagged Product records, the corresponding relationship is:

`tbl_Tags.ProductType → tbl_ProductType.ProductTypeCode`

`ProductTypeTitle` is the human-readable Product Type name. Queries intended for users should display the title rather than an internal code when the title is available.

Product Type identifies the business/process category of a Product and may affect which Action Types are applicable to it.

The available Product Types and their operational behavior depend on the project configuration.

---

# 3. Product Base Information

Before creating a Product, the required base information must be defined.

Product Base Information can include:

- Product Brand
- Product Class
- Product Group
- Product Size
- Product Subgroup
- Product Status
- Product Type
- Product Unit
- Other project-specific base information

These values help classify, identify, search, and manage Products.

Base information is distinct from dynamic technical characteristics. For example, Brand and Product Type are classification attributes, while voltage and power may be technical properties.

The physical table names and relationships confirmed for the Product base-information structure include:

| Business concept | Physical table | Relationship from `tbl_Products` |
|---|---|---|
| Product Brand | `tbl_ProductBrand` | `tbl_Products.fld_ProductBrand → tbl_ProductBrand.fld_ProductBrandCode` |
| Product Group | `tbl_ProductGroup` | `tbl_Products.fld_ProductGroup → tbl_ProductGroup.fld_ProductGroupCode` |
| Product Subgroup | `tbl_ProductSubGroup` | `tbl_Products.fld_ProductSubGroup → tbl_ProductSubGroup.fld_ProductSubGroupCode` |
| Product Class | `tbl_ProductClass` | `tbl_Products.fld_ProductClass → tbl_ProductClass.fld_ProductClassCode` |
| Product Status | `tbl_ProductStatus` | `tbl_Products.ProductStatus → tbl_ProductStatus.ProductStatusCode` |
| Product Type | `tbl_ProductType` | `tbl_Products.ProductType → tbl_ProductType.ProductTypeCode` |
| Product Size | `tbl_ProductPropertyC` | `tbl_Products.ProductSize → tbl_ProductPropertyC.fld_ProductPropertyCId` |

These relationships use the documented business-code or identifier columns. Do not replace them with joins on similarly named `Id` columns unless the authoritative schema explicitly defines that relationship.

The relevant table columns include:

- `tbl_ProductBrand`: `fld_ProductBrandId`, `fld_ProductBrandCode`, `fld_ProductBrandTitle`, `fld_ProductBrandData`
- `tbl_ProductGroup`: `fld_ProductGroupId`, `fld_ProductGroupCode`, `fld_ProductGroupTitle`, `fld_ProductGroupData`
- `tbl_ProductSubGroup`: `fld_ProductSubGroupId`, `fld_ProductSubGroupCode`, `fld_ProductSubGroupTitle`, `fld_ProductSubGroupSubTitle`, `fld_ProductSubGroupDesc`, `fld_ProductGroupCode`
- `tbl_ProductClass`: `fld_ProductClassId`, `fld_ProductClassCode`, `fld_ProductClassTitle`, `fld_ProductClassSubTitle`, `fld_ProductClassDesc`
- `tbl_ProductStatus`: `ProductStatusId`, `ProductStatusCode`, `ProductStatusTitle`, `ProductStatusDesc`
- `tbl_ProductType`: `ProductTypeId`, `ProductTypeCode`, `ProductTypeTitle`, `ProductTypeParentId`, `ProductTypeParentsId`
- `tbl_ProductPropertyC`: `fld_ProductPropertyCIdentity`, `fld_ProductPropertyCId`, `fld_ProductPropertyCTitle`, `fld_ProductPropertyCDesc`, `fld_ProductPropertyCData`, `fld_ProductPropertyCTemp`

The Product Subgroup parent relationship is:

`tbl_ProductSubGroup.fld_ProductGroupCode → tbl_ProductGroup.fld_ProductGroupCode`

Product Size is represented by `tbl_ProductPropertyC`; do not assume that a separate physical table named `ProductSize` exists.

The underlying structure of base information is defined by the database schema. Its displayed business meaning and terminology may vary between projects.

---

# 4. Technical Table Name vs. User-Facing Name

The technical names of database tables remain fixed, while the names displayed to users can be customized for each project.

For example, the Product Group concept may be displayed as:

- `گروه کالا`
- `تیپ خودرو`

Similarly, Product Size may be displayed as:

- `سایز`
- `عرض کالا`
- `گراماژ`

The terminology settings are managed through:

`Strings > Settings`

Changing a term may affect its display throughout the software wherever that configured term is used.

The Agent must distinguish between:

1. The technical database/table concept
2. The user-facing business terminology

A displayed name does not establish that the underlying table or column has changed. The Agent must not infer physical database names from user-facing terminology alone.

For SQL generation, use the physical names documented in the authoritative schema, even when the user refers to a concept by its configured Persian display name.

---

# 5. Product Code

`ProductCode` is the main business identifier of a Product.

The main Product table is `tbl_Products`. Its `ProductCode` identifies the Product definition associated with serialized units.

The primary documented Product-to-Tag relationship is:

`tbl_Tags.ProductCode → tbl_Products.ProductCode`

Conceptually, Product information is associated with a Serial through the Product Code, and the Serial is associated with its Tag.

```text
tbl_Products.ProductCode
          ↑
tbl_Tags.ProductCode

Serial / Tag identity:
tbl_Tags.ProductSerial + tbl_Tags.TagEpc
```

The relevant columns in `tbl_Tags` include `ProductCode`, `ProductSerial`, and `TagEpc`.

The documented business/composite key for a Tag record is:

`(tbl_Tags.ProductSerial, tbl_Tags.TagEpc)`

`tbl_Tags.Id` is an identity column and is not the documented relationship key for Tag records.

Product information associated with the Product Code may include:

- Product Type
- Brand
- Group
- Class
- Size
- Subgroup
- Status
- Other Product characteristics

The Product Code provides the main business connection between a Product definition and the serialized units associated with it.

The system also provides Product search capabilities. Depending on the available configuration, operators may search using part of the Product Title, Product Type, or other Product characteristics to find the required Product.

For SQL generation, the Agent must verify the actual columns and relationships against the authoritative database schema rather than treating the conceptual flow as a complete database join definition.

---

# 6. Technical Code / Nickname

In addition to the main `ProductCode`, the Product table contains the additional identifier:

`ProductTechnicalCode`

This identifier can provide a short or familiar way for operators to identify and search for a Product.

For example, employees may commonly recognize a Product by `2142`, even if its official Product Code is longer.

- `ProductCode`: The primary Product identifier.
- `ProductTechnicalCode`: An additional operational identifier.

These concepts must not be treated as interchangeable.

The physical column `tbl_Products.ProductTechnicalCode` is distinct from `tbl_Products.ProductTechnicalData`.

**Technical Code is not the same as Technical Data.** Technical Code identifies a Product; Technical Data describes its technical characteristics.

The Agent must not assume that Technical Code is always shorter or always exists for every Product.

---

# 7. Automatic Product Code and Title Generation

The system supports automatic generation of `ProductCode` and Product Title based on a predefined formula or algorithm where this functionality is configured.

The operator selects the required base information, such as:

- Size
- Brand
- Class
- Other Product characteristics

The system then generates the Product Code or Title according to the configured rules.

This functionality can help reduce data-entry errors and standardize Product identification and naming.

The exact generation rules depend on the project configuration. The Agent must not assume that automatic generation is enabled for every project.

---

# 8. Shipment / Load Unit

The system uses the concept of `محموله` (Shipment / Load Unit).

A Shipment / Load Unit is the traceable unit registered as a Serial and associated with one or more Tags according to the relevant Product and process configuration.

Its physical meaning can vary between projects. For example, in the tile industry, the registered unit may be a pallet.

A Shipment / Load Unit may contain multiple smaller units and represent a measurable quantity, weight, or volume.

Therefore, `Shipment / Load` should be understood as the registered and traceable unit, rather than assuming it always represents a single item or a specific type of package.

## Product Packaging and Shipment Fields

### Purpose

This section defines the documented business meanings and physical database column mappings of Product packaging and shipment fields in `dbo.tbl_Products`.

### 1. Main Unit

- **Database column:** `tbl_Products.ProductUnit`
- **Business meaning:** واحد اصلی کالا
- **Usage:** Use this column when the user asks about the Product's main unit.

### 2. Second Unit Value

- **Database column:** `tbl_Products.ProductValue`
- **Business meaning:** مقدار هر واحد دوم
- **Usage:** Use this column when the user asks about the quantity or measurable value represented by one second unit.

For example, one carton may represent `1.5 m²`.

### 3. Number of Second Units in a Shipment

- **Database column:** `tbl_Products.ProductCountInPack`
- **Business meaning:** تعداد واحد دوم در محموله
- **Usage:** Use this column when the user asks how many second units are included in a standard shipment or package.

For example, one pallet may contain 45 cartons.

### 4. Shipment Value

- **Database column:** `tbl_Products.ProductPackValue`
- **Business meaning:** مقدار استاندارد محموله
- **Usage:** Use this column when the user asks about the configured total quantity represented by a standard shipment or registered unit.

For example, a pallet containing 45 cartons of `1.5 m²` each has a standard shipment value of `67.5 m²`.

### 5. Shipment Weight

- **Database column:** `tbl_Products.ProductPackWeight`
- **Business meaning:** وزن محموله
- **Usage:** Use this column when the user asks about the configured shipment or package weight.

### 6. Shipment Volume

- **Database column:** `tbl_Products.ProductPackVolume`
- **Business meaning:** حجم محموله
- **Usage:** Use this column when the user asks about the configured shipment or package volume.

### 7. Mandatory SQL Column Mapping Rules

When generating SQL queries, use the following exact mappings:

| User-facing concept | Physical database column |
|---|---|
| واحد اصلی کالا | `tbl_Products.ProductUnit` |
| مقدار هر واحد دوم | `tbl_Products.ProductValue` |
| تعداد واحد دوم در محموله | `tbl_Products.ProductCountInPack` |
| مقدار محموله | `tbl_Products.ProductPackValue` |
| وزن محموله | `tbl_Products.ProductPackWeight` |
| حجم محموله | `tbl_Products.ProductPackVolume` |

These mappings define the documented relationship between business terminology and database columns. They do not authorize the creation or assumption of additional columns.

The Agent must not substitute guessed column names such as `Unit`, `SecondUnit`, `SecondUnitQuantity`, or `ShipmentQuantity` for the documented mappings.

For example:

- For the value of one second unit, use `tbl_Products.ProductValue`, not an assumed `SecondUnit` column.
- For the number of second units in a shipment, use `tbl_Products.ProductCountInPack`, not an assumed `SecondUnitQuantity` column.
- For the configured shipment value, use `tbl_Products.ProductPackValue`, not an assumed `ShipmentQuantity` column.

### 8. Authoritative Schema Validation

Before generating SQL, the Agent must verify every referenced table and column against the actual `CREATE TABLE` definitions in the database schema section beginning with:

`# Application sql server table definitions:`

This section defines business meanings and documented column mappings. The authoritative database schema remains the source of truth for determining whether a table or column exists.

The Agent must never invent a database column, even if a business concept appears in another knowledge document.

If a requested concept cannot be mapped to a column confirmed by the authoritative schema, the Agent must not generate SQL using a guessed column name.

---

# 9. Quantity

The system supports `Quantity`, which can represent either a count or a measurable amount, depending on the Product and project.

Examples include:

- A shipment containing 100 pieces
- A pallet containing a number of cartons
- A shipment measured in square meters

A Product can have a configured standard or default quantity for its registered unit.

During `Register`, the operator selects the Product Code and the system may initialize the registered unit's quantity using the Product's configured default value.

The actual quantity may differ from the standard value when the registered shipment is incomplete, where the process allows the operator to adjust it.

For example, a full tile pallet may represent `67.5 m²`, while an incomplete pallet may contain only `20 m²`.

Therefore:

> A Product's configured default shipment quantity does not necessarily equal the actual quantity of every registered Serial.

For SQL interpretation, the Agent must distinguish the configured Product-level shipment value stored in `tbl_Products.ProductPackValue` from the actual quantity recorded for an individual Serial.

The relevant Serial-level quantity column is:

`tbl_Tags.ProductCount`

The relationship between a Product and its serialized/tagged units is established through:

`tbl_Tags.ProductCode → tbl_Products.ProductCode`

The Agent must not treat `tbl_Tags.ProductCount` and `tbl_Products.ProductPackValue` as interchangeable. They represent different levels of information and may use different quantity conventions.

---

# 10. Second Unit

A `Second Unit` is used when a Product or registered shipment needs to be represented using two related measurement units.

Examples:

### Tile Industry

- Number of cartons
- Square meters

### Paper Industry

- Number of rolls
- Weight or length

The relationship between the second-unit value, the number of second units in a shipment, and the total shipment value must be interpreted separately.

For example:

- One carton represents `1.5 m²`.
- One standard pallet contains 45 cartons.
- The standard pallet value is `67.5 m²`.

The documented Product column mappings are:

- `tbl_Products.ProductValue`: Value represented by one second unit.
- `tbl_Products.ProductCountInPack`: Number of second units in a standard package or shipment.
- `tbl_Products.ProductPackValue`: Total standard quantity represented by the package or shipment.

Where these values are defined consistently, the total can be calculated as:

`ProductValue × ProductCountInPack = ProductPackValue`

This relationship is an example of the expected packaging calculation. The Agent must not assume the values are populated, that the equality holds for every Product, or that every project uses the same measurement convention without supporting data.

Some Products are managed by count only. Other Products require both a count and a measurable amount.

The exact units and conventions depend on the Product and project configuration.

---

# 11. Accounting System Integration

The Product section of Silo can integrate with accounting or ERP systems, including systems such as Rahkaran or Shayegan where the relevant integration is configured.

Product information may be retrieved through a Web Service and added or synchronized in Silo.

This integration can help to:

- Avoid defining the same Product manually in multiple systems
- Reduce duplicate data entry
- Maintain consistency between Product information in connected systems

The Agent must not assume that a specific integration, Web Service, or synchronization process is enabled in every project.

Product import from Excel may also be available where supported. Its availability and behavior must be confirmed for the relevant project.

---

# 12. Technical Data

In addition to Product Base Information and Shipment Information, Silo supports Product-level technical information through the following documented columns in `tbl_Products`:

- `ProductProperties`
- `ProductTechnicalData`

These columns are distinct from `ProductTechnicalCode`.

Technical Data describes technical characteristics that can vary between Products or projects.

Examples include:

- Input voltage
- Device power
- Body color
- Length and width
- Other Product-specific technical characteristics

The documented concept describes Technical Data as dynamic Key/Value data stored in JSON:

```text
Technical Data
    Key → Value
    Key → Value
    Key → Value
```

This approach allows Products to have different technical characteristics without requiring a fixed database column for every possible property.

Technical Data may be used for:

- Label / Tag printing
- Reporting
- Filtering Products
- Retrieving specific technical characteristics

For example, a report may identify inventory containing pumps with a voltage of `24V`.

### Product-Level Data vs. Serial-Level Data

The Agent must distinguish between:

- **Product-level Technical Data:** Describes the Product definition, including the Product-level JSON columns documented in `tbl_Products`.
- **Serial-level information:** Describes a specific registered unit, such as the production furnace used for that Serial, where such information is recorded at Serial level.

The exact storage location and database representation of each type of information must be verified against the authoritative schema. The conceptual distinction alone does not establish a specific table, column, or relationship.

The Agent must not assume that `ProductProperties` and `ProductTechnicalData` have identical meanings or contents. It must not assume that all technical characteristics are stored at Product level or that every project uses the same technical-data structure.

---

# 13. Product Information Categories

Product information in Silo can be understood through three conceptual categories.

## 13.1. Base Information

Base Information contains general Product characteristics such as:

- Brand
- Group
- Class
- Size
- Subgroup
- Status
- Product Type
- Unit
- Other project-specific classification attributes

The relevant physical tables and relationships include:

- `tbl_Products.fld_ProductBrand → tbl_ProductBrand.fld_ProductBrandCode`
- `tbl_Products.fld_ProductGroup → tbl_ProductGroup.fld_ProductGroupCode`
- `tbl_Products.fld_ProductSubGroup → tbl_ProductSubGroup.fld_ProductSubGroupCode`
- `tbl_Products.fld_ProductClass → tbl_ProductClass.fld_ProductClassCode`
- `tbl_Products.ProductSize → tbl_ProductPropertyC.fld_ProductPropertyCId`
- `tbl_Products.ProductStatus → tbl_ProductStatus.ProductStatusCode`
- `tbl_Products.ProductType → tbl_ProductType.ProductTypeCode`

The parent relationship for Product Subgroup is:

`tbl_ProductSubGroup.fld_ProductGroupCode → tbl_ProductGroup.fld_ProductGroupCode`

These values are generally managed through dedicated base-information tables. The Agent must use the correct title columns when displaying human-readable names.

## 13.2. Shipment Information

Shipment Information describes the measurement and packaging defaults associated with the Product's registered unit.

It can include:

- Main Unit
- Second-unit value
- Number of second units in a package
- Standard shipment quantity
- Weight
- Volume

The documented column mappings are:

| Concept | Database column |
|---|---|
| Main Unit | `tbl_Products.ProductUnit` |
| Second-unit value | `tbl_Products.ProductValue` |
| Number of second units in a package | `tbl_Products.ProductCountInPack` |
| Standard shipment quantity | `tbl_Products.ProductPackValue` |
| Shipment weight | `tbl_Products.ProductPackWeight` |
| Shipment volume | `tbl_Products.ProductPackVolume` |

Product-level defaults must not be confused with the actual quantity or other values recorded for an individual Serial.

## 13.3. Product Technical Data

Dynamic Technical Data describes technical characteristics that may vary by Product or project.

The documented Product-level columns include `ProductProperties` and `ProductTechnicalData` in `tbl_Products`. These are separate from `ProductTechnicalCode`.

The documented concept uses JSON Key/Value data and may support label printing, reporting, and filtering.

The Agent must verify the precise meaning and structure of each JSON column against the authoritative schema and any applicable data-format documentation.

---

# 14. IsActive

The Product structure includes an active/inactive concept represented by the physical column:

`tbl_Products.fld_ProductIsActive`

When a Product is no longer produced or used, it may be deactivated rather than deleted.

When a Product is inactive:

- It is not intended for new operational use and may no longer be available for selection.
- Existing Product records and history are not deleted.
- Historical information may remain available in records and reports.

The exact behavior in forms and operations depends on the project implementation.

For SQL generation, use the actual column name `fld_ProductIsActive`, not the conceptual name `IsActive`.

The Agent must verify the column's data type and stored values against the authoritative schema before interpreting a specific record as active or inactive.

---

# 15. HasDoubleTag

The Product structure includes a double-tag requirement represented by the physical column:

`tbl_Products.fld_HasDoubleTag`

When the requirement is enabled for a Product, each relevant Serial is expected to have two Tags according to the configured registration process.

Conceptually:

```text
Double-tag requirement enabled
        ↓
The registered Serial requires two Tags
```

This requirement may apply to specific Products or processes in a project.

The Agent must use `fld_HasDoubleTag` as the physical Product column and verify its data type and stored values against the authoritative schema.

The Agent must not assume that all Products require two Tags or that the presence of this flag alone proves that a specific Serial has two registered Tag records. Actual Tag records must be checked when the question concerns a particular Serial.

---

# 16. Product Database Structure Summary

The following diagram summarizes the conceptual Product information structure and the documented physical relationships.

```text
dbo.tbl_Products
│
├── ProductCode
├── ProductTitle
├── ProductENTitle
├── ProductTechnicalCode
│
├── Product Type
│   └── ProductType
│       → tbl_ProductType.ProductTypeCode
│
├── Product Base Information
│   ├── fld_ProductBrand
│   │   → tbl_ProductBrand.fld_ProductBrandCode
│   ├── fld_ProductGroup
│   │   → tbl_ProductGroup.fld_ProductGroupCode
│   ├── fld_ProductSubGroup
│   │   → tbl_ProductSubGroup.fld_ProductSubGroupCode
│   ├── fld_ProductClass
│   │   → tbl_ProductClass.fld_ProductClassCode
│   ├── ProductSize
│   │   → tbl_ProductPropertyC.fld_ProductPropertyCId
│   └── ProductStatus
│       → tbl_ProductStatus.ProductStatusCode
│
├── Shipment Information
│   ├── ProductUnit
│   ├── ProductValue
│   ├── ProductCountInPack
│   ├── ProductPackValue
│   ├── ProductPackWeight
│   └── ProductPackVolume
│
├── Product-Level Data
│   ├── ProductProperties (JSON)
│   └── ProductTechnicalData (JSON)
│
├── ProductRegUser
│   → tbl_User.Id
│
├── fld_ProductIsActive
└── fld_HasDoubleTag
```

The Product-to-Tag relationship is:

```text
tbl_Products.ProductCode
    ↑
tbl_Tags.ProductCode
```

The Tag record is identified for the documented business relationship by the combination:

```text
tbl_Tags.ProductSerial + tbl_Tags.TagEpc
```

This is a conceptual and relationship summary, not a complete physical database schema.

It does not establish that every item above is a separate table, that all projects use the same terminology, or that every concept is stored in the same table.

Only tables, columns, and relationships confirmed by the authoritative database schema may be used in SQL queries.

---

# 17. Product → Serial → Tag Relationship

Registering a Serial or Tag requires the corresponding Product to exist and be identifiable.

The main Product-to-Tag relationship is:

`tbl_Tags.ProductCode → tbl_Products.ProductCode`

The general conceptual flow is:

```text
Product
   ↓
tbl_Products.ProductCode
   ↑
tbl_Tags.ProductCode

Serialized/tagged unit:
tbl_Tags.ProductSerial + tbl_Tags.TagEpc
```

The Product Code identifies the Product definition associated with the Serial. The Serial is then associated with its Tag or Tags according to the relevant process.

The Product structure provides the identity, classification, and configured packaging defaults needed by the registration process.

Other documented relationships in `tbl_Tags` include:

- `tbl_Tags.fld_ProductBrand → tbl_ProductBrand.fld_ProductBrandCode`
- `tbl_Tags.fld_ProductGroup → tbl_ProductGroup.fld_ProductGroupCode`
- `tbl_Tags.fld_ProductSubGroup → tbl_ProductSubGroup.fld_ProductSubGroupCode`
- `tbl_Tags.fld_ProductClass → tbl_ProductClass.fld_ProductClassCode`
- `tbl_Tags.ProductType → tbl_ProductType.ProductTypeCode`
- `tbl_Tags.ProductStatus → tbl_ProductStatus.ProductStatusCode`
- `tbl_Tags.fld_ProductPropertyCId → tbl_ProductPropertyC.fld_ProductPropertyCId`
- `tbl_Tags.fld_ProductPropertyAId → tbl_ProductPropertyA.fld_ProductPropertyAId`
- `tbl_Tags.fld_ProductPropertyBId → tbl_ProductPropertyB.fld_ProductPropertyBId`
- `tbl_Tags.TagRegisterUser → tbl_User.Id`
- `tbl_Tags.ProductCode → tbl_Products.ProductCode`

The Product-level classification columns in `tbl_Products` represent the Product definition. Some similarly named classification columns in `tbl_Tags` are denormalized snapshots associated with serialized/tagged units. The Agent must not assume that the values in `tbl_Tags` and `tbl_Products` are always synchronized or interchangeable without verifying the relevant data and application behavior.

The Tag table also contains the following documented fields related to other parts of the warehouse system:

- `TagInDestinationId`
- `TagInActionId`
- `TagInDestinationId2`
- `TagInActionId2`
- `TagZone`
- `fld_InspectActionId`
- `fld_LastInspectResult`

These fields belong to Destination, Action, location, and inspection workflows, which are documented separately. Their detailed behavior should not be inferred from this Product document.

The conceptual flow does not, by itself, define every physical database join. Use the explicit relationships documented here and verify additional joins against the authoritative schema.

---

# 18. Important Database Tables and Relationships

The following physical table names and relationships are documented for the Product structure.

## 18.1. Main Product and Base-Information Tables

| Physical table | Relevant columns | Purpose / relationship |
|---|---|---|
| `dbo.tbl_Products` | `ProductCode`, `ProductTitle`, `ProductTechnicalCode`, `ProductType`, `ProductStatus`, `ProductSize`, `ProductUnit`, packaging fields, Product JSON fields, classification fields | Main Product definition |
| `dbo.tbl_ProductBrand` | `fld_ProductBrandId`, `fld_ProductBrandCode`, `fld_ProductBrandTitle`, `fld_ProductBrandData` | Brand information |
| `dbo.tbl_ProductGroup` | `fld_ProductGroupId`, `fld_ProductGroupCode`, `fld_ProductGroupTitle`, `fld_ProductGroupData` | Group information |
| `dbo.tbl_ProductSubGroup` | `fld_ProductSubGroupId`, `fld_ProductSubGroupCode`, `fld_ProductSubGroupTitle`, `fld_ProductSubGroupSubTitle`, `fld_ProductSubGroupDesc`, `fld_ProductGroupCode` | Subgroup information and parent Group relationship |
| `dbo.tbl_ProductClass` | `fld_ProductClassId`, `fld_ProductClassCode`, `fld_ProductClassTitle`, `fld_ProductClassSubTitle`, `fld_ProductClassDesc` | Class information |
| `dbo.tbl_ProductStatus` | `ProductStatusId`, `ProductStatusCode`, `ProductStatusTitle`, `ProductStatusDesc` | Product Status information |
| `dbo.tbl_ProductType` | `ProductTypeId`, `ProductTypeCode`, `ProductTypeTitle`, `ProductTypeParentId`, `ProductTypeParentsId` | Product Type classification |
| `dbo.tbl_ProductPropertyC` | `fld_ProductPropertyCIdentity`, `fld_ProductPropertyCId`, `fld_ProductPropertyCTitle`, `fld_ProductPropertyCDesc`, `fld_ProductPropertyCData`, `fld_ProductPropertyCTemp` | Product Size information |
| `dbo.tbl_ProductPropertyA` | `fld_ProductPropertyAId`, `fld_ProductPropertyATitle`, `fld_ProductPropertyADesc`, `fld_ProductPropertyAData` | Production-line dimension |
| `dbo.tbl_ProductPropertyB` | `fld_ProductPropertyBId`, `fld_ProductPropertyBTitle`, `fld_ProductPropertyBDesc`, `fld_ProductPropertyBData`, `fld_ProductPropertyAId` | Shift dimension, associated with Property A |
| `dbo.tbl_Tags` | `ProductSerial`, `ProductCode`, `TagEpc`, `ProductCount`, classification snapshots, property IDs | Serialized/tagged unit information |
| `dbo.tbl_User` | `Id` | User relationship for Product registration and Tag registration |
| `dbo.tbl_Gallery` | `fld_GalleryId`, `fld_GalleryUserId`, `fld_GalleryMediaName`, `fld_GalleryMediaPath`, `fld_GalleryUsageType`, `fld_GalleryUploadDateTime`, `fld_GalleryUsageId`, `fld_GalleryMediaExtensionType`, `fld_GalleryAdditionalData` | Product image gallery; stores media file information, including file name and storage path. |

## 18.2. Exact Product Relationships

The following relationships must be used when joining `tbl_Products` to its documented base-information tables:

```text
tbl_Products.fld_ProductBrand
    → tbl_ProductBrand.fld_ProductBrandCode

tbl_Products.fld_ProductGroup
    → tbl_ProductGroup.fld_ProductGroupCode

tbl_Products.fld_ProductSubGroup
    → tbl_ProductSubGroup.fld_ProductSubGroupCode

tbl_Products.fld_ProductClass
    → tbl_ProductClass.fld_ProductClassCode

tbl_Products.ProductType
    → tbl_ProductType.ProductTypeCode

tbl_Products.ProductStatus
    → tbl_ProductStatus.ProductStatusCode

tbl_Products.ProductSize
    → tbl_ProductPropertyC.fld_ProductPropertyCId

tbl_Products.ProductRegUser
    → tbl_User.Id

tbl_Products.ProductGalleryId
    → tbl_Gallery.fld_GalleryId
The ProductGalleryId field in tbl_Products references the fld_GalleryId field in dbo.tbl_Gallery. The gallery record contains the media file name in fld_GalleryMediaName and the storage path in fld_GalleryMediaPath.
```

The documented Product Subgroup parent relationship is:

```text
tbl_ProductSubGroup.fld_ProductGroupCode
    → tbl_ProductGroup.fld_ProductGroupCode
```

The main Product-to-Tag relationship is:

```text
tbl_Tags.ProductCode
    → tbl_Products.ProductCode
```

The documented Tag-to-base-information relationships include:

```text
tbl_Tags.fld_ProductBrand
    → tbl_ProductBrand.fld_ProductBrandCode

tbl_Tags.fld_ProductGroup
    → tbl_ProductGroup.fld_ProductGroupCode

tbl_Tags.fld_ProductSubGroup
    → tbl_ProductSubGroup.fld_ProductSubGroupCode

tbl_Tags.fld_ProductClass
    → tbl_ProductClass.fld_ProductClassCode

tbl_Tags.ProductType
    → tbl_ProductType.ProductTypeCode

tbl_Tags.ProductStatus
    → tbl_ProductStatus.ProductStatusCode

tbl_Tags.fld_ProductPropertyAId
    → tbl_ProductPropertyA.fld_ProductPropertyAId

tbl_Tags.fld_ProductPropertyBId
    → tbl_ProductPropertyB.fld_ProductPropertyBId

tbl_Tags.fld_ProductPropertyCId
    → tbl_ProductPropertyC.fld_ProductPropertyCId

tbl_Tags.TagRegisterUser
    → tbl_User.Id
```

`tbl_ProductPropertyB.fld_ProductPropertyAId` associates Property B with Property A.

`tbl_ProductPropertyC.fld_ProductPropertyCIdentity` is the table's identity/primary-key column, while `fld_ProductPropertyCId` is the documented relationship column used by Product Size and Tag property references. Do not substitute one for the other.

`tbl_Tags.Id` is an identity value and is not the documented business key for Tag relationships. The relevant Tag identity is the combination of `ProductSerial` and `TagEpc`.

**Schema validation requirement:** These documented relationships must be checked against the authoritative `CREATE TABLE` definitions when additional constraints, data types, or relationships are required. Do not invent foreign keys or replace documented relationship columns with similarly named columns.

---

# 19. Important Agent Rules

When answering questions about the Silo Product database structure, the Agent must follow these rules:

1. `dbo.tbl_Products` is the physical main Product table. Do not use `tbl_Product` as its physical name.
2. A Product must exist before its Serial or Tag can be registered.
3. Required Product Base Information must exist before the Product can be properly defined.
4. `tbl_Products.ProductCode` is the main Product identifier associated with serialized/tagged units through `tbl_Tags.ProductCode`.
5. The documented Tag business/composite key is `(tbl_Tags.ProductSerial, tbl_Tags.TagEpc)`. Do not use `tbl_Tags.Id` as a substitute relationship key.
6. `ProductTechnicalCode` is distinct from `ProductCode` and Technical Data.
7. Product Type classifies a Product from a business/process perspective and may affect applicable Action Types.
8. Product base-information concepts include Brand, Group, Class, Size, Subgroup, Status, and other project-specific attributes.
9. Use the documented physical relationships:
   - `tbl_Products.fld_ProductBrand → tbl_ProductBrand.fld_ProductBrandCode`
   - `tbl_Products.fld_ProductGroup → tbl_ProductGroup.fld_ProductGroupCode`
   - `tbl_Products.fld_ProductSubGroup → tbl_ProductSubGroup.fld_ProductSubGroupCode`
   - `tbl_Products.fld_ProductClass → tbl_ProductClass.fld_ProductClassCode`
   - `tbl_Products.ProductType → tbl_ProductType.ProductTypeCode`
   - `tbl_Products.ProductStatus → tbl_ProductStatus.ProductStatusCode`
   - `tbl_Products.ProductSize → tbl_ProductPropertyC.fld_ProductPropertyCId`
   - `tbl_Products.ProductRegUser → tbl_User.Id`
10. The documented Product Subgroup parent relationship is `tbl_ProductSubGroup.fld_ProductGroupCode → tbl_ProductGroup.fld_ProductGroupCode`.
11. Technical table names and user-facing terminology must be distinguished. Do not infer database names from displayed labels.
12. User-facing terminology is documented as configurable through `Strings > Settings`; the physical schema must still be verified independently.
13. `ProductProperties` and `ProductTechnicalData` are separate columns in `tbl_Products`. Do not assume they have identical contents or meanings.
14. Technical Data may be used for label printing, reporting, and filtering where supported.
15. Product deactivation is represented by `tbl_Products.fld_ProductIsActive`. Do not substitute a guessed `IsActive` column.
16. A double-tag requirement is represented by `tbl_Products.fld_HasDoubleTag`. Verify actual Tag records when determining whether a specific Serial has two Tags.
17. `tbl_Products.ProductUnit` represents the main Product unit.
18. `tbl_Products.ProductValue` represents the value of one second unit.
19. `tbl_Products.ProductCountInPack` represents the number of second units in a standard package or shipment.
20. `tbl_Products.ProductPackValue` represents the configured standard quantity of a package or shipment.
21. `tbl_Products.ProductPackWeight` represents the configured shipment weight.
22. `tbl_Products.ProductPackVolume` represents the configured shipment volume.
23. Product-level default shipment quantity must not be confused with `tbl_Tags.ProductCount`, which records quantity at the serialized/tagged-unit level.
24. The relationship `ProductValue × ProductCountInPack = ProductPackValue` describes the expected packaging calculation when the Product's values and units are defined consistently; do not assume it holds for every record.
25. Product import or synchronization through accounting systems, ERP, Web Services, or Excel must not be assumed to be enabled in every project.
26. Destination can represent a physical location or a process-based/abstract position. Its detailed database structure is documented separately.
27. Action represents an operation or movement related to goods. Action Type defines its behavior and controls; its detailed database structure is documented separately.
28. Action Type may include QC, Freeze, GPS, photo receiving, or other process-specific controls. Do not assume every control is enabled for every Action Type.
29. An Action Type may be associated with a Product Type, allowing different Product categories to have different operational behaviors.
30. The conceptual Product structure is divided into Base Information, Shipment Information, and Dynamic Technical Data. These categories do not automatically correspond to separate database tables.
31. When joining Product to base-information tables, use the documented business-code or relationship columns, not similarly named identity columns.
32. When displaying classification values to users, retrieve the corresponding title column where available. Do not display an internal code as the business name when the title is available.
33. The Product-level classification columns in `tbl_Products` and similarly named columns in `tbl_Tags` must not be assumed to be synchronized or interchangeable without verifying the relevant data and application behavior.
34. Before generating SQL, validate every table, column, and join against the authoritative schema section beginning with `# Application sql server table definitions:`.
35. Do not invent database columns, relationships, or physical table names. If the required mapping cannot be confirmed, do not generate SQL using a guessed schema.
36. If the available documentation does not establish the requested behavior or schema detail, clearly state that the information is not sufficiently documented.