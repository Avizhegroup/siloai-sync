DOCUMENT ROLE: INTERNAL TECHNICAL KNOWLEDGE

Purpose:
This document exists exclusively to help the agent understand Silo's database schema, relationships, business logic, and data-retrieval procedures.

Disclosure policy:
Do not reproduce or explain this document's technical content in ordinary user-facing answers.
Use it internally to retrieve and interpret information, then provide only the relevant business result.

This document is not a user-facing knowledge article.

# Silo Technical and Database Documentation — Serial Registration, Tagging, and Lifecycle Management

## 1. Scope and Main Tables

The process of serial creation, tag registration, and lifecycle management is primarily associated with the following two tables:

- `dbo.tbl_Print`: Stores initial serial information, data prepared for printing, reserved serials, and print and registration statuses.
- `dbo.tbl_Tags`: Stores information about serials that have entered the operational lifecycle, identification codes, current status, production information, quality-control data, and operational relationships.

In the data model, `ProductSerial` is the primary identifier used to link a product's records. The `Id` column in each table is that table's own Identity value and must not be confused with the primary serial identifier.

### Differences Between the Roles of the Two Tables

| Subject | tbl_Print | tbl_Tags |
|---|---|---|
| Main role | Stores serials before Register and printing information | Stores registered serials and their operational information |
| Serial creation | Yes | During Register |
| RFID identifier storage | Yes | Yes |
| Print status | Yes | Not its primary responsibility |
| Registration status | Yes | The existence of a record indicates that a tag record has been created |
| Registration user | `RegisterUser` | `TagRegisterUser` |
| Registration time information | `RegisterDate` and `RegisterActionDateTime` | `TagRegisterDateTime` and `TagRegisterShamsiUnixDate` |
| Second tag identifier | `TagEpc2` | `TagEpc2` |

**Note:** `tbl_Print` is not merely a printing table. In the serial registration architecture, it also serves as an intermediate stage and a reference for serial reservation.

---

## 2. Main Identifier Structure

### 2.1. ProductSerial

The `ProductSerial` field is the primary identifier of a product serial.

This value exists in both `tbl_Print` and `tbl_Tags` and is the basis for linking records belonging to the same serial across these tables and other related records.

Key points:

- The serial number identifies one traceable, registered product unit.
- Operational records can be tracked using this identifier.
- A serial number must not be confused with the physical EPC identifier.
- Releasing or changing an EPC must not result in the deletion of the serial's history.

### 2.2. Id

In both tables, `Id` is an Identity column.

It identifies a record within its own table and must not be used as the primary key for linking serial-related records.

### 2.3. TagEpc

The `TagEpc` field stores the physical identification identifier associated with a serial.

This value may contain one of the following:

- An EPC identifier stored on an RFID tag
- A dedicated product barcode
- The product serial number, when the same number is used as the barcode identifier

This field exists in both of the following tables:

- `tbl_Print.TagEpc`
- `tbl_Tags.TagEpc`

**Note:** In the supplied DDL, `tbl_Tags.TagEpc` is defined as `nvarchar(50) NOT NULL`. Therefore, the explanation from the training session regarding an empty EPC must be verified against the actual data structure. The value may be stored as an empty string, or the database structure may differ in another environment. Do not assume that this field can contain `NULL` without checking.

---

## 3. Serial Creation and Registration Lifecycle in the Database

### 3.1. Serial Creation and Reservation Stage

During the initial stage, serial information is inserted into `tbl_Print`.

Important related fields:

| Field | Purpose |
|---|---|
| `ProductSerial` | Serial number |
| `ProductCode` | Product code |
| `ProductName` | Product title |
| `ProductDescription` | Product description |
| `ProductType` | Product type |
| `ProductCount` | Product quantity |
| `ProductItemCount` | Number of items |
| `ProductCountInPack` | Quantity of products per package |
| `ProductUnit` | Product unit |
| `ProductSize` | Product size |
| `ProductRegCode` | Product registration abbreviation code |
| `ProductWeight` | Weight |
| `ProductVolume` | Volume |
| `ProductStatus` | Status or characteristic defined for the product |
| `ProductProductionShift` | Production shift |
| `ProductProductionLine` | Production line |
| `ProductProperties` | Dynamic product information |
| `TagEpc` | Identification identifier |
| `TagEpc2` | Second identification identifier |

At this stage, a serial can be created and even printed without its corresponding record having been created in `tbl_Tags`.

### 3.2. Serial Uniqueness Control

In the standard approach, serial number generation and reservation take place in `tbl_Print`.

The purpose is to ensure that even if a serial is created but is not registered for any reason, its number will not be assigned to another serial again.

If the serial-generation formula is implemented dynamically and outside the standard reservation mechanism, another independent mechanism must be defined to guarantee uniqueness.

The intermediate table is also important in scenarios involving Excel imports and data exchange with external systems.

### 3.3. Register Stage

During registration, serial information moves from the preparation stage into the operational structure.

In the standard process:

1. Serial information is available in `tbl_Print`.
2. The registration type, barcode-based or RFID-based, is determined.
3. The required identification method is selected.
4. The corresponding serial record is created in `tbl_Tags`.
5. Registration status information is updated in the corresponding `tbl_Print` record.

### 3.4. Related Backend Methods

In the integrated process structure, the main methods are:

- `SSavePrint`: Creates and saves serial information in the printing and preparation stage.
- `SRegisterTag`: Performs serial and tag registration.

Based on the training-session information, `SRegisterTag` receives parameters representing the following concepts:

| Parameter | Meaning |
|---|---|
| `ProductSerial` | Serial number |
| `ProductCode` | Product code |
| `ProductCount` | Product quantity |
| `EPCList` | List of EPC identifiers |
| `UserName` | Requesting user |
| `DeviceID` | Device identifier |
| `DeviceIP` | Device information or a value related to the process |
| `RegisterProperty` | Additional registration information |

These names belong to the method parameters and are not necessarily the names of database columns.

### 3.5. Serial Creation During Register

In the Android form «ثبت با کد کالا» (Register by Product Code), a serial may not yet exist in `tbl_Print`.

In this situation:

1. The operator enters the product code and the required quantity.
2. Required dynamic information, such as production line, shift, and warehouse, is specified.
3. The request is sent to `SRegisterTag`.
4. The method detects that `ProductSerial` has not already been provided.
5. `SSavePrint` is called first.
6. Serial numbers are generated and inserted into `tbl_Print`.
7. The Register process immediately continues.
8. The corresponding records are inserted into `tbl_Tags`.

Therefore, in this scenario, serial creation and registration are two consecutive stages within a single workflow.

---

## 4. Updating tbl_Print After Register

After a record is inserted into `tbl_Tags`, the corresponding record in `tbl_Print` is also updated.

Based on the training-session explanations, the important fields involved in this update are:

| Actual DDL Field | Purpose |
|---|---|
| `RegisterUser` | User who performed registration |
| `TagEpc` | EPC or identification identifier |
| `TagEpc2` | Second identifier |
| `PrintFlag` | Print status |
| `RegisterFlag` | Registration status |
| `RegisterDate` | Registration date |

The `RegisterActionDateTime` field also exists in the table structure and stores time information related to the registration process.

### Benefits of This Update

**Benefit 1: Preserving Records**

Even if a problem later occurs with records in the tag table, the registration user, registration time, and initial identifiers can still be examined in the print table.

**Benefit 2: Reducing Reporting Costs**

In high-transaction projects, the number of records in both tables can be very large.

If a simple report of registered and unregistered serials requires a costly Join between the two tables every time, the processing cost increases.

By updating `RegisterFlag` and other related fields in `tbl_Print`, registration status can be retrieved directly from that table.

**Note:** The actual registration status must be verified against the execution logic and the values assigned to these fields. The presence of a value in `TagEpc` alone is not sufficient proof that the Register process has been completed.

---

## 5. TagEpc and TagEpc2 Logic

### 5.1. TagEpc

The primary identification field exists in both tables:

- `tbl_Print.TagEpc`
- `tbl_Tags.TagEpc`

This field may contain an RFID identifier or a dedicated barcode.

In the standard barcode-based mode, the serial number itself can be used as the barcode identifier, so there is no need to assign a separate RFID identifier.

### 5.2. Equivalent Tag

In this method, the EPC identifier of the first tag is written onto the chip of the second tag.

The fundamental difference between this method and Double Tag is that both physical tags receive the same EPC at the hardware level.

Database implications:

- The serial number does not change.
- No new serial is created.
- No new record is created in `tbl_Tags` for the second tag.
- The serial's `TagEpc` value does not change.
- The physical chip identifier of the second tag becomes identical to the `TagEpc` value of the first tag.

In this process, the main operation is performed on the RFID chip memory. It must not be confused with registering an independent EPC in `TagEpc2`.

### 5.3. Double Tag

This capability is used to assign two independent identifiers to one serial.

In the supplied DDL, the related fields are:

- `tbl_Print.TagEpc`
- `tbl_Print.TagEpc2`
- `tbl_Tags.TagEpc`
- `tbl_Tags.TagEpc2`

In this method:

- The first EPC is stored in `TagEpc`.
- The second EPC is stored in `TagEpc2`.
- Both identifiers belong to the same `ProductSerial`.
- The physical identifier on the second tag is not changed.
- Either tag can be used to retrieve the same serial.

This capability is enabled at the product specification level through the `fld_HasDoubleTag` field in `tbl_Product`.

During Android registration, if this capability is enabled for the product, the operator must provide two independent EPC identifiers.

**Important limitation:** The Double Tag capability discussed here supports a maximum of two independent identifiers per serial.

### 5.4. Technical Differences Between the Two Methods

| Characteristic | Equivalent Tag | Double Tag |
|---|---|---|
| ProductSerial | Same | Same |
| Physical EPC | Same | Different |
| TagEpc | Primary identifier | First identifier |
| TagEpc2 | Not necessarily involved | Second identifier |
| Chip modification | Yes | No |
| Creation of a second record in tbl_Tags | No | No |
| Implementation method | Writing the EPC onto the second chip | Registering two independent EPCs |

---

## 6. Serial Lifecycle and EPC Release

The core architectural principle is that a serial's identity is independent of its physical identification device.

`ProductSerial` represents the permanent history of the product, whereas `TagEpc` and `TagEpc2` are identification and association mechanisms.

In projects that use reusable RFID tags, the same EPC may be assigned to different serials at different times.

Example:

1. EPC A is assigned to serial 1002.
2. Serial 1002 completes its operational lifecycle.
3. At a defined stage of the lifecycle, the system releases the EPC.
4. The EPC reference is removed from the previous serial.
5. The same EPC is assigned to a new serial.

In this process, the previous serial record is not deleted.

### Related Fields

- `tbl_Tags.ProductSerial`
- `tbl_Tags.TagEpc`
- `tbl_Tags.TagEpc2`
- `tbl_Print.ProductSerial`
- `tbl_Print.TagEpc`
- `tbl_Print.TagEpc2`

This value is a project-specific example and must not be treated as a fixed, universal status across all Silo projects.

A distinction must also be maintained between releasing the physical identifier and deleting the serial number.

**Technical note:** In the supplied DDL, `tbl_Tags.TagEpc` is defined as NOT NULL. Therefore, the EPC release mechanism must be checked against the type of value actually stored in the production environment. An empty value in business logic does not necessarily mean that `NULL` is stored.

### Bulk Registration Error and EPC Association Change

During bulk registration, an RFID reader may incorrectly read an EPC identifier belonging to one serial and register it against another serial.

If the operation is finalized after the operator confirms it, the EPC association with the original serial may be removed, and the same EPC may be assigned to the second serial.

Database implications:

- The first serial's record remains in `tbl_Tags`.
- The second serial is also registered.
- The EPC identifier may no longer be associated with the first serial.
- As a result, a serial may remain without an assigned RFID identifier.

When investigating such cases, the current association between `ProductSerial` and `TagEpc` must be checked. The existence of a serial record alone must not be considered proof that an RFID identifier is still assigned to it.

---

## 7. Reviewing the tbl_Tags Field Structure

This section explains the fields related to the training session according to their technical responsibilities.

### 7.1. Serial Identifier and Basic Information

| Field | Description |
|---|---|
| `Id` | Record Identity identifier |
| `ProductSerial` | Primary serial identifier |
| `ProductCode` | Product code |
| `ProductName` | Product title |
| `ProductCount` | Quantity assigned to the serial |
| `ProjectCode` | Project code; no specific operational use is identified in the current standard logic |
| `TagEpc` | Primary identification identifier |
| `TagEpc2` | Second identification identifier |

### 7.2. Information Copied from Product Specifications

The following fields were introduced during the session as information associated with product specifications:

| Actual Field | Meaning |
|---|---|
| `ProductType` | Product type |
| `ProductStatus` | Characteristic or status defined for the product |
| `ProductName` | Product title |
| `fld_ProductGroup` | Product group |
| `fld_ProductBrand` | Product brand |
| `fld_ProductSubGroup` | Product subgroup |
| `fld_ProductClass` | Product class |
| `fld_ProductPropertyCId` | Identifier of Property C associated with product specifications |
| `RegCode` | Product abbreviation code |

These fields represent basic product attributes rather than attributes inherently belonging to an individual serial.

Some of these values are duplicated in the tag table to avoid Joins or preserve historical information.

**Note:** The displayed titles of these fields may differ across projects.

### 7.3. TagStatus

Field:

`tbl_Tags.TagStatus`

This field stores the serial's operational status and is set according to the type of its current destination.

Its meaning is different from a general product characteristic.

In Silo's architecture, destinations have specific types, such as:

- Serial documentation issued
- Under reconstruction
- Reconstruction completed
- Delivered to customer

When the serial's destination changes, the `TagStatus` value is also updated according to the new destination type.

`TagStatus` was introduced as equivalent to the concept of DestinationType.

The exact value and its mapping to destination types must be obtained from the actual project configuration and data. They must not be inferred from the field value alone.

### 7.4. Registration Date and User

| Field | Purpose |
|---|---|
| `TagRegisterShamsiUnixDate` | Registration date and time in the format used by the system |
| `TagRegisterDateTime` | Registration date and time of type datetime |
| `TagRegisterUser` | Identifier of the user who performed registration |
| `Username` | User information stored in the record |
| `DeviceId` | Identifier of the device or registration station |

During the session, an example combining the Persian calendar date and time was mentioned for `TagRegisterShamsiUnixDate`.

This field must be interpreted according to the actual format of the stored data. It must not be converted to a standard Unix Timestamp in seconds without verification.

### 7.5. Device Information and Original Barcode

Field:

`tbl_Tags.DeviceIp`

According to the explanation provided during the session, the current use of this field is to store the original product serial number or barcode, not necessarily the actual device IP address.

Example: Fanavaran Tahvieh Payam project

- A pump has an original 12-digit barcode.
- The Silo serial number is created as a 24-digit value by combining the product code with the original barcode.
- The original barcode is stored in `DeviceIp` to preserve its history.

Therefore, when analyzing data from projects that use this approach, `DeviceIp` must not be assumed to represent the device IP address without verification.

The `DeviceId` field stores the identifier of the registration device or station.

### 7.6. Dynamic Serial Information

Field:

`tbl_Tags.ProductProperties`

This field stores dynamic information in JSON format.

Information entered by the operator during Register may be stored in this field as a collection of Key-Value pairs.

Conceptual example:

```json
{
  "ProductionOrder": "12345",
  "BatchNumber": "B-100",
  "CustomField": "Value"
}
```

This example illustrates the conceptual structure only. Actual keys must be obtained from the dynamic-field configuration and project data.

The `tbl_Print` table also contains a `ProductProperties` field for storing dynamic information during the preparation stage.

### 7.7. Production Line and Shift Information

Actual fields:

- `tbl_Tags.fld_ProductPropertyAId`
- `tbl_Tags.fld_ProductPropertyBId`

These fields are related to the production line or production hall and the work shift.

The reference tables introduced were:

- `tbl_ProductPropertyA`: Stores Property A information, associated with the production line or production hall.
- `tbl_ProductPropertyB`: Stores work-shift information. Its exact table name is not identified in the available information.

These fields do not belong to the product code itself. They store information about the location and conditions under which the serial was created.

### 7.8. Property C

Actual field:

`tbl_Tags.fld_ProductPropertyCId`

This field was associated with product size or another product-related characteristic.

Unlike Properties A and B, Property C is dependent on the product's basic specifications.

### 7.9. ProductCount

Field:

`tbl_Tags.ProductCount`

This field stores the quantity of the product associated with a serial.

The distinction between the number of serials and the quantity of products is very important.

Examples:

| Scenario | Number of Serials | ProductCount |
|---|---:|---:|
| One tile pallet | 1 | 80 |
| One stone block | 1 | 24 |
| One box of blades | 1 | 40 |
| One pump | 1 | 1 |

Reports must preserve the distinction between these two concepts:

- **Unique Serial Count:** The count of distinct `ProductSerial` values.
- **Total Product Quantity:** The sum of `ProductCount`.

`COUNT` and `SUM` must not be used interchangeably.

### 7.10. ProductWeight and ProductVolume

Fields:

- `tbl_Tags.ProductWeight`
- `tbl_Tags.ProductVolume`

These fields store the actual weight and volume assigned to a serial.

These values may differ from the default values defined in the product specifications.

Main uses:

- Product placement calculations
- Product collection calculations
- Management of actual serial quantities

### 7.11. Status and Control Information

| Field | Meaning |
|---|---|
| `Lock` | Quality rejection or lock status |
| `Freeze` | Serial freeze or blocked status |
| `Deactivate` | A field that, according to the session, is not currently used in the implemented logic |
| `TagStatus` | Serial operational status |
| `TagZone` | Serial location |
| `fld_LastInspectResult` | Latest inspection result |
| `fld_InspectActionId` | Identifier of the latest inspection operation |

### 7.12. Lock and Quality Control

Field:

`tbl_Tags.Lock`

According to the session explanation, activating this field indicates that the serial has been placed in a quality-rejected or quality-locked state.

Operational controls and validations can use this status to identify QC rejection. Therefore, under the logic described, it is not necessary to query inspection tables solely to determine this status.

This does not mean that inspection history is deleted. Detailed inspection records may still be stored in the relevant module.

### 7.13. Freeze

Field:

`tbl_Tags.Freeze`

This field indicates whether a serial is frozen.

Freeze and Unfreeze operations are performed through the relevant module.

When a movement operation is executed, if the Freeze control is enabled and the serial's `Freeze` value is active, the operation must stop with the corresponding freeze error.

**Note:** According to the session, `Deactivate` is not used for this logic.

### 7.14. Inspection Information

Field:

`tbl_Tags.fld_LastInspectResult`

This field stores the latest inspection result in JSON format.

Since a serial may be inspected multiple times, this field stores the latest result, not necessarily the complete historical inspection record.

Field:

`tbl_Tags.fld_InspectActionId`

This field stores the identifier of the latest inspection operation.

This identifier belongs to an inspection operation and must not be confused with a movement operation identifier.

---

## 8. Destination Structure and Latest Movement Operation

Two important fields are:

- `tbl_Tags.TagInDestinationId`
- `tbl_Tags.TagInActionId`

### 8.1. TagInDestinationId

This field stores the serial's current destination.

During a movement operation:

1. The serial is retrieved from its previous destination.
2. The movement operation is executed.
3. The new destination is determined.
4. `TagInDestinationId` is updated.
5. `TagStatus` is also updated according to the type of the new destination.

According to the session explanation, this field must always identify the current destination in the operational serial structure.

### 8.2. TagInActionId

This field stores the identifier of the latest movement transaction.

**Very important:**

`TagInActionId` is related to the `tbl_MovementActions` table and must not be confused with `ActionType`.

The difference is:

- **ActionType:** Defines the operation type and its rules, such as sending products to sales.
- **MovementActionID:** Identifies the actual movement transaction created when the operation is executed.

For example, if a serial is included in movement action number 12322, that transaction identifier is stored in `TagInActionId`.

### 8.3. Secondary Fields

The following fields were described during the session as having no current use:

- `tbl_Tags.TagInActionId2`
- `tbl_Tags.TagInDestinationId2`

Do not assign an operational purpose to these fields without additional documentation.

---

## 9. Serial Relationships and Parent-Child Structures

Silo has two independent concepts for relationships between serials:

1. Transferring a quantity from one serial to another.
2. Grouping serials within a packaging structure.

These two concepts must not be combined in technical documentation.

### 9.1. Quantity Transfer and TagTreeParentsId

Field:

`tbl_Tags.TagTreeParentsId`

This field is used to maintain the parent-child relationship when transferring a quantity from one serial to another.

Example:

- Serial A has a quantity of 40.
- A quantity of 10 is separated from it.
- A new serial B is created for the separated quantity.
- The relationship identifying serial A as the parent of serial B is stored.

This structure makes it possible to display the serial genealogy and trace the origin of the transferred quantity.

This capability is different from the packaging system.

### 9.2. Unused Fields or Fields Without a Current Defined Purpose

According to the session explanation:

- `TagTreeParentId`: Not currently used.
- `TagTreeSecondParentId`: Not currently used.
- `NewProductSerial`: Its current purpose has not been identified.

Do not replace `TagTreeParentsId` with these fields without examining the actual execution logic.

### 9.3. Packaging Levels and TagTreeParentsEpc

Field:

`tbl_Tags.TagTreeParentsEpc`

This field stores the EPC identifier of the parent package.

### 9.4. TagTreeParentSerial

Field:

`tbl_Tags.TagTreeParentSerial`

This field stores the serial number of the parent package.

These two fields are used in the packaging structure.

Example:

- There are 10 independent serials for 10 cartons.
- A master pallet is defined for these cartons.
- The EPC or serial number of the master pallet is recorded in the corresponding child records.
- Scanning the parent identifier makes it possible to retrieve the associated child serials.

### Differences Between the Two Structures

| Characteristic | Quantity Transfer | Packaging |
|---|---|---|
| Purpose | Separating product quantity | Grouping products |
| Main fields | `TagTreeParentsId` | `TagTreeParentsEpc` and `TagTreeParentSerial` |
| Relationship | Origin of the quantity assigned to a new serial | Parent package and its child items |
| Example | Transferring 10 units from a package of 40 | Placing 10 cartons inside one pallet |

---

## 10. Location Information

Field:

`tbl_Tags.TagZone`

This field stores the serial's location within the placement module.

Product placement in Silo is serial-based. Therefore, location information is associated with a specific serial number.

`TagZone` must not be treated as equivalent to the operational destination stored in `TagInDestinationId`.

The difference is:

- `TagInDestinationId`: The serial's operational destination.
- `TagZone`: The serial's physical location within the placement structure.

---

## 11. Other tbl_Tags Fields

The following fields also exist in the supplied DDL:

| Field | Session Explanation |
|---|---|
| `RegCode` | Product abbreviation code |
| `ContractStatus` | Contract status; case-specific and project-dependent |
| `fld_LastModifierUser` | Last modifying user; according to the session, it is probably not currently populated by the implemented logic |
| `TagRegisterDateTime` | Exact registration time |
| `ReProduct` | Not currently used |
| `Temp` | Its purpose was not identified during the session |

### Difference Between TechnicalCode and RegCode

According to the session explanation:

- `TechnicalCode` in the product table represents the product abbreviation code.
- `RegCode` in `tbl_Tags` stores the same concept for the serial record.
- The corresponding field in the supplied `tbl_Print` DDL is `ProductRegCode`.

Do not confuse `RegCode` with `ProductCode` or `ProductSerial`.

---

## 12. Information Related to tbl_Print

Other fields in `tbl_Print` are used to manage printing, registration, and data-entry workflows.

### 12.1. Printing Fields

| Field | Purpose |
|---|---|
| `PrintActionId` | Print operation group identifier |
| `PrintType` | Print type |
| `PrintActionDateTime` | Print operation time |
| `PrintUser` | User who performed printing |
| `PrintFlag` | Print status |
| `PrintQueue` | Print queue status |

`PrintActionId` is used to group records belonging to the same print request.

This identifier must not be confused with `MovementActionID`.

### 12.2. Registration Fields

| Field | Purpose |
|---|---|
| `RegisterFlag` | Registration status |
| `RegisterType` | Registration type |
| `RegisterActionDateTime` | Registration operation time |
| `RegisterUser` | User who performed registration |
| `RegisterDate` | Registration date |
| `TagEpc` | First identifier |
| `TagEpc2` | Second identifier |

---

## 13. Troubleshooting Exit Operations

This section is particularly important for the technical agent.

When a user reports that a serial is not allowed to exit, the first step is to identify the exact operation they mean.

The word "exit" alone does not identify the operation type.

### 13.1. Identifying the ActionType

For example, the user may be referring to the «ارسال به فروش» (Send to Sales) operation.

In this case, the intended operation type must first be identified.

The configuration of that specific operation type must then be examined:

- Operation source
- Operation destination
- Active controls
- Serial eligibility conditions
- Permitted document statuses
- Document status transition rules

Controls belonging to one operation type must not be generalized to another operation.

### 13.2. Source Presence Control

The information required for verification includes:

- Serial number
- Current serial destination in `tbl_Tags.TagInDestinationId`
- Operational status in `tbl_Tags.TagStatus`
- Source defined for the ActionType
- Source-presence control in the configuration of that ActionType

If the source-presence control is enabled and the serial's current destination does not match the permitted operation source, the operation must be rejected.

Possible solutions:

- Move the serial to the permitted source.
- Have the system administrator review the control configuration.

Do not draw conclusions from `TagStatus` alone without understanding the destination mapping.

### 13.3. Freeze Control

Required information:

`tbl_Tags.Freeze`

If this field is active and the Freeze control is enabled for the relevant ActionType, the movement operation must be rejected.

Possible solutions:

- Investigate the reason for the freeze.
- Execute the Unfreeze process if the issue has been resolved.
- Have the system administrator review the control configuration.

### 13.4. QC and Lock Control

Required information:

`tbl_Tags.Lock`

If this field indicates a quality rejection or quality lock, and the corresponding control is enabled in the ActionType, the operation must be rejected.

Possible solutions:

- Review the inspection result.
- Complete the QC process.
- Record the verified result.
- Have the system administrator review the control configuration.

`Lock` is important for identifying the current quality-rejection status. To view the latest inspection result, `fld_LastInspectResult` must also be examined.

### 13.5. Distinguishing ActionType from MovementActionID

During error analysis, the following two concepts must be kept separate:

- **ActionType** defines the rules under which an operation is permitted.
- **MovementActionId** identifies the actual movement transaction.

The `TagInActionId` field stores the latest movement transaction, not the operation type identifier.

Therefore, `TagInActionId` must not be interpreted as ActionType when identifying the active operation controls.

### 13.6. Recommended Troubleshooting Sequence

1. Retrieve the relevant `ProductSerial`.
2. Check whether the serial record exists in `tbl_Tags`.
3. Examine `TagInDestinationId` and `TagStatus`.
4. Identify the intended ActionType.
5. Examine the source and destination defined for that ActionType.
6. Check the source-presence control.
7. Check `Freeze` and the Freeze control.
8. Check `Lock` and the QC control.
9. Examine other active controls for the same operation.
10. Report the cause only when supported by actual evidence.

### 13.7. How the Agent Should Respond to the User

If the operation type is not specified, the agent should first ask:

«منظورتان از خروج کالا، کدام عملیات است؟ برای مثال ارسال به فروش یا عملیات دیگری؟»

After the operation has been identified, the agent must determine the cause based on the actual serial information and the active controls for that operation.

If the cause is the serial's absence from the source, this must be stated explicitly.

If the cause is a freeze, the agent must explain that the serial is frozen and describe the effect of the operation's Freeze control.

If the cause is QC rejection, the agent must explain the quality-control status and the relevant active operation condition.

The agent must not conclude that a field is the cause of an error merely because the field exists. The corresponding control must also be verified as enabled in the ActionType configuration.

---

## 14. Mandatory Rules for Database Analysis

The following rules are essential for preventing incorrect data interpretation:

1. `ProductSerial` is the primary serial identifier; `Id` is the record's Identity value.
2. `TagEpc` is an identification mechanism and is not the same as the serial number.
3. `TagEpc2` is used for the second identifier in the Double Tag capability.
4. Equivalent Tag differs from Double Tag: in Equivalent Tag, the second EPC is written onto the chip; in Double Tag, two independent EPCs are stored.
5. Releasing an EPC does not mean deleting the serial.
6. `ProductCount` represents product quantity, not the number of serials.
7. `TagStatus` represents the serial's operational status and must not be confused with `ProductStatus`.
8. `TagInDestinationId` identifies the current destination.
9. `TagInActionId` identifies the latest movement transaction, not the operation type.
10. `Freeze` and `Lock` are two different concepts.
11. `TagTreeParentsId` represents the relationship that tracks the origin of a quantity transfer.
12. `TagTreeParentsEpc` and `TagTreeParentSerial` are used for the parent-child packaging structure.
13. `TagZone` represents physical location and is different from the operational destination.
14. `fld_ProductPropertyAId` and `fld_ProductPropertyBId` store production and shift information.
15. `fld_ProductPropertyCId` is related to product specifications.
16. `ProductProperties` stores dynamic serial information.
17. To determine whether a serial has been registered, the actual `RegisterFlag` logic and the corresponding `tbl_Tags` record must be examined.
18. For reporting, use distinct serial counts to calculate the number of serials and the sum of `ProductCount` to calculate total product quantity.

---

## 15. Final Principle for Using the Two Knowledge Bases

These two documents complement each other, but they have different audiences and purposes.

| Subject | Conceptual Knowledge Base | Technical Knowledge Base |
|---|---|---|
| Register | Registration concepts and stages | `SRegisterTag` and table changes |
| Serial reservation | Purpose and use | `tbl_Print.ProductSerial` |
| RFID registration | How a tag is associated with a product | `TagEpc` |
| Barcode registration | Using the serial as the barcode | `TagEpc` and `RegisterType` |
| Equivalent Tag | Procedure and use cases | Writing the EPC onto the second chip |
| Double Tag | Difference between the two tags and implementation | `TagEpc`, `TagEpc2`, and `HasDoubleTag` |
| Tag release | Reusable tag lifecycle | Clearing or changing the EPC association |
| Serial quantity | Difference between serial count and quantity | `ProductCount` |
| Dynamic attributes | Serial-specific information | `ProductProperties` |
| Operational status | Product's current stage | `TagStatus` and `TagInDestinationId` |
| Freeze | Blocking concept | `Freeze` and ActionType controls |
| QC | Quality rejection concept | `Lock` and `fld_LastInspectResult` |
| Quantity transfer | Separating part of a product quantity | `TagTreeParentsId` |
| Packaging | Relationship between a package and its items | `TagTreeParentsEpc` and `TagTreeParentSerial` |
| Product placement | Physical product location | `TagZone` |
| Exit operation errors | Troubleshooting guidance | Serial, destination, and ActionType control checks |

**Agent Usage Rules:**

- When responding to regular users, use the concepts and workflows described in the first document.
- When analyzing data, investigating errors, troubleshooting, or generating technical reports, use the information in the second document.
- Technical information should only be included in user-facing responses when the user explicitly asks about technical details.
- The technical document must not cause the agent to include internal table and field names in ordinary user responses.
- Neither document should present information that is merely assumed or has not yet been verified against the database and execution logic as an established fact.