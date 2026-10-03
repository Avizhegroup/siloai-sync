# Serial Generation and Tag Registration — Technical Structure

## 1. Overall Architecture of Serial Generation and Registration

The serial generation process in the Silo system is separate from the tag registration process.

In the first stage, the serial number is created in the Print table.

In the second stage, after registration is performed, the serial becomes a real entity in the Tag table, and the RFID identifier or EPC is associated with it.

Therefore, the existence of a serial in the Print table alone does not mean that an active inventory entity exists in the system.

---

## 2. Print Table

The Print table is the main reference for serial number generation and allocation.

Information such as the following is stored in this table:

* Serial Number
* Product Code
* Code Title
* Description
* Type
* Quantity
* Item Count
* Unit
* Size
* Short Code

### Quantity and Item Count

The Quantity and Item Count fields can represent different concepts.

For example, in the tile industry:

* Quantity can represent the total pallet area.
* Item Count can represent the number of cartons contained in the pallet.

### Short Code

The Short Code is used to store a short name for the product.

For example:

`SMBS`

can be used as the short code for Sodium Metabisulfite (سدیم متابی‌سولفیت).

---

## 3. Tag Table

The Tag table is where the serial entity is registered after the registration process.

After registration, the Tag record contains the serial information and the RFID identifier or EPC.

Therefore:

**Print = Generated serial waiting for subsequent steps**

**Tag = Registered serial with a tag entity**

---

## 4. Why Print Is Used as the Serial Reference

Serial generation and RFID registration do not necessarily happen at the same time.

A serial may be created and its label printed, while the RFID tag may be registered several hours later.

Therefore, if the Tag table is used as the reference for the latest generated serial, a serial that has already been generated and printed but has not yet been registered will not be considered when calculating the next serial number.

This can result in duplicate serial numbers.

For this reason, the reference for serial generation and allocation must be the Print table.

---

## 5. PrintFlag and RegisterFlag

The Print table contains two important fields:

* PrintFlag
* RegisterFlag

These fields indicate the printing and registration status.

For a newly created serial that has not yet been printed or registered, the status may be:

```text
PrintFlag = 0
RegisterFlag = 0
EPC = Empty
```

In this state, the serial exists in Print but has not yet been registered in Tag.

---

## 6. EPC

EPC is the unique RFID identifier that is associated with the serial during registration.

Before registration, the Print record may not contain an EPC.

After registration, the EPC is stored in the record associated with the serial.

---

## 7. Relationship Between Print and Tag

The main data flow between these two tables is as follows:

**Serial Creation**

→ Create record in Print

→ Print or prepare the label

→ RFID Registration

→ Create record in Tag

→ Update the Print record

Therefore, Print and Tag have different but related roles.

---

## 8. Technical Registration Cycle

After registration is completed, three main changes occur in the system.

### 8.1. Creating the Tag Record

A new record is created in the Tag table.

This record contains the serial number and EPC.

### 8.2. Updating Print

The corresponding record in the Print table is updated.

The EPC is stored in the record and the registration status is updated.

### 8.3. Creating TagMovement

Depending on the destination warehouse, a record is created in the tag movement or TagMovement structure.

This record represents the entry of the product into the corresponding warehouse.

---

## 9. TagMovement

TagMovement is used to record product and tag movements.

During registration, based on the destination warehouse, a movement record is created for the serial's entry into that warehouse.

Therefore, registration not only creates the Tag entity, but also creates the corresponding inventory status and movement.

---

## 10. Warehouse and Active Serial Entity

According to the system logic, an active serial cannot exist without a warehouse.

Therefore, the destination warehouse is part of the serial generation and registration process.

After registration, the serial entity must be associated with a specific warehouse.

---

## 11. ActionType

The registration path and product destination depend on ActionType configuration.

An operation can be defined in ActionType where the source is `-1`.

In the system logic, source `-1` represents the state before registration or the state where the serial has not yet been placed in a warehouse.

The movement:

`-1 → Destination`

is considered a registration operation.

---

## 12. Relationship Between ActionType and ProductType

ActionTypes can be defined based on ProductType.

For example, an organization may have the following product types:

* Finished Product
* Raw Material
* Semi-Finished Product

Each of these types can have different ActionTypes and destinations.

For example:

```text
Finished Product → Finished Product Warehouse
Raw Material → Raw Material Warehouse
Semi-Finished Product → Related Warehouse
```

As a result, a project may have multiple registration ActionTypes with source `-1`.

---

## 13. DestinationType

The destination type, or DestinationType, also plays a role in determining the product path.

A factory may have multiple types of products with different paths, and each product type may be directed to a different destination after registration.

---

## 14. ProductType

In the current system structure, ProductType is designed as a static field.

This field plays a role in selecting the ActionType and determining the operational path of the product.

Therefore, completely removing or changing ProductType is not possible without reviewing and redesigning its existing dependencies.

---

## 15. DocCodeSet

An ActionType contains a setting called DocCodeSet.

If DocCodeSet is enabled, a document number becomes mandatory for the operation.

In this case, if the user does not enter the document number, the operation cannot be registered.

---

## 16. DocumentItemCheck

The DocumentItemCheck setting is used to validate document items.

When this option is enabled:

1. The document number is validated.
2. The document items are retrieved from the related system.
3. The requested products are matched against the document items.
4. The requested quantity is compared with the quantity allowed by the document.

For example, if:

* The product does not exist in the document.
* The requested quantity is greater than the quantity available in the document.

The system must prevent the operation from continuing.

---

## 17. Integration with External Systems

In some projects, document information and its items are received from external systems such as Rahkaran (راهکاران).

The system can match the received information against the serial issuance request.

---

## 18. Quality Control

ActionType settings can also be used for processes that require quality control.

For example, receiving a finished product into the finished-product warehouse may require quality approval or inspection.

In contrast, another product type such as raw materials may be registered without quality control.

Therefore, registration and tag-registration rules can differ for different ProductTypes and ActionTypes.

---

## 19. Serial Generation Formula

Serial generation can use a calculation formula.

For example, a formula can consist of a combination of:

```text
Product Code + Date + Counter
```

The result can be a serial number with a specific length, such as a 24-digit serial number.

---

## 20. Limitation of Static Formulas

In the previous structure, some serial-generation formulas were implemented directly in the application code.

This approach means that creating a custom formula for a project requires:

* Changing the application code.
* Building the software again.
* Publishing the software.
* Installing the new version in the customer's environment.

---

## 21. Desired Serial Formula Structure

The desired structure is to store the serial-generation formula in the database.

With this approach, it becomes possible to:

* Define a new formula.
* Change a project-specific formula.
* Configure the formula through a user interface.
* Create a new serial-generation logic without changing the application code.
* Change the serial-generation logic without republishing the software.

---

## 22. Dynamic Fields in the System Architecture

From an architectural perspective, Dynamic Fields can be defined for three areas:

### Product

General product attributes and product definition information.

### Serial

Attributes specific to each individual product unit.

### Operation

Information required when executing an operation.

This architecture makes it possible to use a dynamic structure instead of creating a large number of fixed tables or columns for different attributes.

---

## 23. Registration Using a Handheld Device

In some projects, registration is performed using a Handheld Device (دستگاه هندهلد).

In this case, in addition to reading the RFID, the device's location information may also be recorded.

For example, in a vehicle-related project, the device GPS was required during registration due to security requirements.

---

## 24. Windows and Web Versions

Some system capabilities were previously implemented as Windows Forms applications.

These included:

* Registration
* Gate

In the newer architecture, some of these capabilities have been migrated to the Web version.

The Web version allows changes to be applied centrally on the server and reduces the need to separately update users' systems.

---

## 25. Complete Technical Flow

The complete technical flow of serial generation and registration is as follows:

```text
Serial Issuance Request
        ↓
Select Product
        ↓
Select Production Line
        ↓
Select Work Shift
        ↓
Select Destination Warehouse
        ↓
Select Document if Required
        ↓
Set Quantity
        ↓
Complete Dynamic Fields
        ↓
Generate Serial Number
        ↓
Create Print Record
        ↓
PrintFlag / RegisterFlag
        ↓
Print Label
        ↓
Read or Associate RFID
        ↓
RegisterTag
        ↓
Create Tag Record
        ↓
Store EPC
        ↓
Update Print Record
        ↓
Create TagMovement
        ↓
Register Inventory in Destination Warehouse
```

---

## 26. Important Architectural Principle

The key principle in this process is:

**The Print table is the reference for serial number generation, while the Tag table is the reference for the registered serial entity.**

The existence of a serial in Print alone does not mean that it has been registered.

Registration is considered complete when the serial has been associated with an RFID tag, the Tag record has been created, the Print record has been updated, and the corresponding warehouse movement has been recorded.
