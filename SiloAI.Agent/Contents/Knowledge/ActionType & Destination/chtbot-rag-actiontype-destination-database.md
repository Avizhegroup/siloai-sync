DOCUMENT ROLE: INTERNAL TECHNICAL KNOWLEDGE

Purpose:
This document exists exclusively to help the agent understand Silo's database schema, relationships, business logic, and data-retrieval procedures.

Disclosure policy:
Do not reproduce or explain this document's technical content in ordinary user-facing answers.
Use it internally to retrieve and interpret information, then provide only the relevant business result.

This document is not a user-facing knowledge article.

# Silo — Technical Database Knowledge: Action Types, Destinations, and Operational Controls

## 1. Purpose and General Rules

This document defines the known technical structure and relationships related to action types, destinations, destination types, product types, operational controls, documents, RFID, and destination permissions for the Silo agent.

The agent must use this information to retrieve data, construct queries, and analyze operations. In ordinary user-facing responses, do not expose table names, column names, internal identifiers, or JSON structures unless the user explicitly requests a technical explanation.

**Core Rules:**

- Column names must exactly match the actual database schema.
- Distinguish documented relationships from potential or unverified relationships.
- The existence of a column alone does not establish its complete runtime behavior or the uniqueness of its values.
- The actual tag state, action type configuration, and destination permissions are three separate concerns.
- Never guess unknown values, `NULL` values, or unverified mappings.

---

## 2. Action Type Table — `dbo.tbl_ActionTypes`

This table stores action type configurations.

| Actual Column Name | Data Type | Purpose |
|---|---|---|
| `Id` | `int IDENTITY(1,1) NOT NULL` | Internal identifier generated when a record is inserted |
| `fld_ActionTypeId` | `int NULL` | Operational action type identifier |
| `fld_ActionTypeTitle` | `nvarchar(50) NULL` | Action type title |
| `fld_ActionTypeFromDestinationType` | `nvarchar(50) NULL` | Source destination type |
| `fld_ActionTypeToTypeDestinationType` | `nvarchar(50) NULL` | Target destination type |
| `fld_ActionTypeProductType` | `nvarchar(50) NULL` | Product type code associated with the action |
| `fld_ActionTypePermitedDocStatus` | `nvarchar(50) NULL` | Permitted document status configuration |
| `fld_ActionTypeChangeDocStatus` | `nvarchar(50) NULL` | Document status change configuration |
| `fld_ActionTypeActiveControls` | `nvarchar(max) NULL` | Active control settings stored as JSON |
| `fld_ActionTypeRfidPower` | `int NULL` | RFID power configuration |
| `fld_ActionTypeChangeTagLocation` | `int NULL` | Configuration controlling changes to a tag's location or zone information |

### Difference Between Identifiers

`Id` and `fld_ActionTypeId` are two different columns. The first is an `IDENTITY` column. However, in the provided DDL, neither a primary key constraint on `Id` nor a uniqueness constraint on `fld_ActionTypeId` has been defined.

The known station relationship uses `fld_ActionTypeId`. Do not treat these two identifiers as interchangeable. Before relying on `fld_ActionTypeId` being unique, verify the actual data.

---

## 3. Destinations, Destination Types, and Tags

### 3.1. Destination — `dbo.tbl_Destination`

A destination represents a specific operational location or destination point.

| Actual Column Name | Purpose |
|---|---|
| `DestinationCode` | Destination code |
| `DestinationType` | Destination type code |
| `DestinationEpc` | JSON data containing destination settings and permissions |

### 3.2. Destination Type — `dbo.tbl_DestinationType`

The known destination type column is:

`fld_DestinationTypeCode`

Relationship:

`tbl_Destination.DestinationType → tbl_DestinationType.fld_DestinationTypeCode`

**Important distinction:** `DestinationCode` identifies a specific destination, while `DestinationType` and `fld_DestinationTypeCode` refer to the destination type classification. These concepts are not interchangeable.

### 3.3. Current Recorded Destination of a Tag — `dbo.tbl_Tags`

The column that stores the tag's currently recorded destination is:

`TagInDestinationId`

Relationship:

`tbl_Tags.TagInDestinationId → tbl_Destination.DestinationCode`

The combination of `ProductSerial` and `TagEpc` is the business key used to identify a tag in the documented data model. Do not assume that `Id` can replace this combination in relationships.

The destination recorded for a tag does not necessarily represent its exact physical location within a zone, aisle, or shelf.

### 3.4. Source and Target Destination Types in an Action Definition

The following columns in `tbl_ActionTypes` store the source and target destination types for an action:

- `fld_ActionTypeFromDestinationType`
- `fld_ActionTypeToTypeDestinationType`

Conceptual relationship:

`From Destination Type → Action Type → To Destination Type`

Both columns have the data type `nvarchar(50)`. Do not assume their values are numeric identifiers or confirmed codes from another table without examining the actual data. Join them to the destination type table only after their value mapping has been verified.

---

## 4. Relationship Between Action Types and Product Types

Product type table:

`dbo.tbl_ProductType`

Known structure:

| Actual Column Name | Data Type | Purpose |
|---|---|---|
| `ProductTypeId` | `int IDENTITY(1,1) NOT NULL` | Internal product type identifier |
| `ProductTypeTitle` | `nvarchar(50) NULL` | Product type title |
| `ProductTypeParentId` | `nvarchar(10) NULL` | Value associated with the parent product type |
| `ProductTypeParentsId` | `nvarchar(max) NULL` | Information associated with product type ancestors |
| `ProductTypeCode` | `nvarchar(50) NULL` | Product type code |

### Correct JOIN Relationship

The action type must be joined to the product type through `ProductTypeCode`:

```sql
LEFT JOIN dbo.tbl_ProductType AS PT
    ON AT.fld_ActionTypeProductType = PT.ProductTypeCode
```

In this example:

- `AT` is the alias for `dbo.tbl_ActionTypes`.
- `PT` is the alias for `dbo.tbl_ProductType`.
- The correct join condition is `AT.fld_ActionTypeProductType = PT.ProductTypeCode`.
- **Do not use `ProductTypeId` for this JOIN.**

To display the product type title, use `PT.ProductTypeTitle`. Do not display the product type code in place of its title.

The provided DDL does not establish a uniqueness constraint on `ProductTypeCode`. Before assuming a one-to-one relationship, check whether duplicate codes exist.

A value in `fld_ActionTypeProductType` may indicate a configured association between an action type and a product type. However, whether the action is ultimately permitted still depends on the system's configuration and runtime logic.

---

## 5. Relationship Between Action Types and Stations

### 5.1. Station — `dbo.tbl_Station`

Known relationship:

`tbl_Station.fld_StationActionType → tbl_ActionTypes.fld_ActionTypeId`

To determine which action type is configured for a station, join `fld_StationActionType` to `fld_ActionTypeId`, not to `Id`.

---

## 6. Document Status and Document Status Changes

The following two columns in `dbo.tbl_ActionTypes` store document-related configuration:

- `fld_ActionTypePermitedDocStatus`: The configured permitted document status value.
- `fld_ActionTypeChangeDocStatus`: The configured document status change value.

The spelling `Permited` is part of the actual column name and must not be corrected in queries.

Both columns have the data type `nvarchar(50)`. Do not assume that their stored values are status titles or identifiers from another table without verifying the actual data and application logic.

The existence of these columns alone does not prove that every action requires a document or changes a document's status. The runtime conditions under which these settings are applied must be verified from the application logic.

---

## 7. Active Action Controls — `fld_ActionTypeActiveControls`

The active control settings are stored in the following column of `dbo.tbl_ActionTypes`:

`fld_ActionTypeActiveControls`

The column has the data type `nvarchar(max)` and stores JSON data.

### 7.1. Rule for Determining Whether a Control Is Active

Report a control as active only when the corresponding JSON key has the exact string value `"true"`.

- `"false"` does not mean the control is active.
- A missing key, `null`, or an unknown value does not prove that the control is active.
- The actual JSON Boolean `true` without quotation marks is not the same as the string `"true"`. Do not treat it as active unless the storage contract has been verified.
- Do not replace invalid or unreadable JSON with a default value.

### 7.2. Documented JSON Keys

| JSON Key | Documented Meaning |
|---|---|
| `NotInSourceIgnore` | Ignore tags that are not at the expected source. |
| `NotInSourceError` | Raise an error when a tag is not at the expected source. |
| `Freezed` | Apply the control associated with a frozen tag or product. |
| `FailedQC` | Apply restrictions to products that failed quality control. |
| `NotQc` | Apply restrictions to products that have no quality control result. |
| `NotRegisterIgnore` | Ignore unregistered tags or serials. |
| `NotRegisterError` | Require the tag or serial to be registered; otherwise, raise an error. |
| `TagLifeLessThanIgnore` | Ignore tags whose lifetime is below the configured minimum. |
| `DocCodeSet` | Require the document number or code to be provided. |
| `DocItemsCheck1` | Check for an exact match between the identified items and the items in the document. |
| `DocItemsCheck2` | Check that the product exists in the document and that the cumulative processed quantity does not exceed the document's total quantity. |
| `DocItemsCheck3` | The exact meaning has not yet been verified. Do not guess its behavior. |

### 7.3. Rules for Reporting Active Controls

When answering the question, "Which controls are active for this action type?":

1. Identify the action type using its actual title in `fld_ActionTypeTitle`.
2. If the title matches multiple records, resolve the ambiguity using the actual data.
3. Read `fld_ActionTypeActiveControls` from the relevant record.
4. Report only documented keys whose values are exactly `"true"`.
5. Explain their meanings according to the table above.
6. If the settings cannot be retrieved or interpreted, state that the active controls cannot be verified.

The fact that a control is active does not, by itself, prove that a specific tag has violated that rule.

---

## 8. Tag Location Change Setting

Actual column:

`fld_ActionTypeChangeTagLocation` — `int NULL`

This setting determines how the tag's zone or location information is handled after an action:

- When enabled, the tag's zone or location information is cleared so that it can be determined again during the relevant process.
- When disabled, the previous location information is retained.

The numeric values corresponding to enabled and disabled must be verified against the application logic. The `int` data type alone does not establish that `1` means enabled and `0` means disabled. Do not assume that `NULL` is equivalent to either zero or one without documentation.

Changing a tag's operational destination is not the same as changing its zone or physical location information.

---

## 9. RFID Power Setting

Actual column:

`fld_ActionTypeRfidPower` — `int NULL`

This column stores the configured RFID power associated with an action type.

Read the value from the actual record for the relevant action type. Do not assume that all actions use the same power setting or that the stored value necessarily equals the actual power applied by the device. Do not replace `NULL` with a guessed default value.

---

## 10. Destination-Level Permissions

Destination permissions are stored in the following JSON column of `dbo.tbl_Destination`:

`DestinationEpc`

Documented keys:

| Key | Meaning |
|---|---|
| `AllowCancelTag` | Permission to cancel a tag |
| `AllowReprintTag` | Permission to reprint a label |
| `AllowEditInformation` | Permission to edit tag information |
| `AllowEditTagValue` | Permission to edit the tag value |

To check a permission, identify the tag's current recorded destination through `tbl_Tags.TagInDestinationId → tbl_Destination.DestinationCode`, then inspect the JSON data for that destination.

Interpret each key according to its actual stored format and value. Do not apply the `"true"` string rule used for `fld_ActionTypeActiveControls` to `DestinationEpc` without verification.

Destination permissions are independent of action type controls. For example, permission to cancel a tag does not imply permission to transfer or edit it.

---

## 11. Tag State and Analysis of Operation Rejections

When analyzing an operation, keep the actual tag state separate from action type settings and destination permissions.

The following two states are different:

- `Lock = true`: A locked state associated with the quality control rejection process.
- `Freeze = true`: A frozen state.

Neither state alone proves that all actions are prohibited. Their effects must be evaluated against the relevant active control and the actual conditions of the operation.

To investigate why an action cannot be performed on a tag:

1. Identify the tag using `ProductSerial` or `TagEpc`, using both if necessary.
2. Retrieve its actual state and currently recorded destination.
3. Identify the action type and read the actual settings for the relevant record.
4. Check the active controls.
5. If destination permissions are relevant, check the permissions for the current destination separately.
6. Report only causes supported by the actual data and documented rules.

Do not provide a generic list of possible causes instead of examining the actual data.

---

## 12. Reference of Known JOIN Relationships

| Source Table | Source Column | Target Table | Target Column |
|---|---|---|---|
| `tbl_Station` | `fld_StationActionType` | `tbl_ActionTypes` | `fld_ActionTypeId` |
| `tbl_Destination` | `DestinationType` | `tbl_DestinationType` | `fld_DestinationTypeCode` |
| `tbl_Tags` | `TagInDestinationId` | `tbl_Destination` | `DestinationCode` |
| `tbl_ActionTypes` | `fld_ActionTypeProductType` | `tbl_ProductType` | `ProductTypeCode` |

This table lists the relationships documented in the system knowledge. It does not guarantee that corresponding `FOREIGN KEY` constraints exist in the DDL. Code uniqueness must also be verified separately.

**Confirmed product type relationship:** `fld_ActionTypeProductType` must be joined to `ProductTypeCode`, not to `ProductTypeId`.

---

## 13. Final Rules for the Agent

- Identify the actual action type first, then read the settings from the corresponding record.
- Do not treat `Id` and `fld_ActionTypeId` as interchangeable.
- Distinguish a destination, a destination type, and a tag's physical location.
- For the product type JOIN, use `fld_ActionTypeProductType = ProductTypeCode`.
- Determine whether a control is active only from the actual JSON data for the relevant action type.
- Evaluate tag state, action type controls, and destination permissions independently.
- Do not interpret document status values, numeric location-change settings, or JSON structures without supporting documentation.
- Do not guess the meaning of `DocItemsCheck3`.
- If duplicate codes or multiple matching records exist, evaluate their effect on the query.
- Never invent an unverified table name, column name, identifier, relationship, or runtime behavior.

**Final Principle:** Conclusions must be based on the actual record, the correct relationship, and the documented meaning. If sufficient information is unavailable to verify a relationship or cause, explicitly preserve that uncertainty.