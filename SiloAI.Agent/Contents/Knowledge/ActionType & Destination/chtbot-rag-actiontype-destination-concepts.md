# Silo — Core Concepts: نوع عملیات, انبار, نوع انبار, and نوع کالا

## 1. What Is Silo?

Silo is a system for identifying goods and recording changes and movements throughout different operational processes. It can provide unique identification and operational history tracking for different entities, including goods and machinery.

Silo is not merely a system for manually recording the movement of one or more serial numbers. It uses identification tools and operational equipment to identify groups of goods and record operations through automated processes.

Goods identification methods in Silo include:

- Identifying tags using RFID technology
- Identifying goods using barcode scanners
- Using identification gates
- Using a handheld device to read tags and perform operations

These capabilities are intended to simplify the identification of goods and the recording of related operations across different processes.

## 2. What Is نوع عملیات?

**نوع عملیات** describes a specific type of operation performed on goods.

In Silo, a suitable نوع عملیات is defined for each required operational movement so that the movement of goods can be identified and recorded.

Examples include:

- Moving goods from the production line to the finished goods warehouse
- Sending goods to sales
- Returning goods to production
- Moving raw materials to the production line

Each نوع عملیات has a specific purpose and may have its own applicable conditions and controls.

In simple terms:

**نوع عملیات = The definition of a specific operation and the conditions associated with performing it**

### 2.1. The Relationship Between نوع عملیات, Source, and Destination

A نوع عملیات is defined based on the type of the source warehouse and the type of the destination warehouse, rather than necessarily being limited to a specific physical warehouse.

For example, the operation "برگشت کالا به تولید" represents a defined operational route between the relevant warehouse types.

When the source and destination are identified, the appropriate نوع عملیات for that route can be determined.

If multiple specific warehouses belong to the same warehouse type, the same نوع عملیات may be applicable to movements between them, provided that the other operational conditions are also satisfied.

### 2.2. Using نوع عملیات to Group Operations

نوع عملیات also helps classify operations in addition to defining the type of movement.

For example, operations that represent the return of goods may belong to a particular operational group, making them easier to identify and track.

### 2.3. Conditions for Performing a نوع عملیات

Each نوع عملیات may have specific execution conditions. These conditions may relate to:

- The state of the goods
- Whether the goods are at the expected source
- Whether the goods are registered
- Quality control status
- Whether the goods are frozen
- Whether a related document exists
- Whether the goods and their quantities match the document

The conditions required for a particular operation depend on the actual configuration of that نوع عملیات. Do not assume that all operations have the same requirements.

## 3. What Is Destination?

**انبار** represents a specific destination in Silo where goods are located or to which they are assigned.

An انبار does not necessarily mean a traditional physical warehouse. Depending on Silo's operational structure, a destination may represent a warehouse, a production line, or another position in a business process.

Examples include:

- انبار مواد اولیه ۱
- انبار محصول
- انبار فروش
- خط تولید ۱
- خط تولید ۲

Each of these can represent a specific destination.

During goods tracking, a good is assigned to a specific destination at a particular stage of the process, and its movements between destinations can be tracked.

## 4. What Is نوع انبار?

**نوع انبار** is used to classify different destinations into broader categories.

For example, the following destinations may exist:

- انبار مواد اولیه ۱
- انبار مواد اولیه ۲

Both destinations may belong to the نوع انبار "انبار مواد اولیه."

In this example:

- **نوع انبار:** Raw Materials Warehouse
- **Specific destinations:** انبار مواد اولیه ۱ and انبار مواد اولیه ۲

Other examples:

| نوع انبار | Example Destinations |
|---|---|
| خط تولید | Production Line 1, Production Line 2 |
| انبار محصول | Finished Goods Warehouse 1, Finished Goods Warehouse 2 |
| انبار فروش | Specific destinations associated with the sales warehouse |

These examples explain the concept of classification. Whether a particular destination actually exists in Silo must be verified against the system's real data.

### 4.1. Difference Between Destination and Destination Type

The distinction between these concepts is important:

- **انبار:** A specific destination
- **نوع انبار:** The category to which a destination belongs

Therefore, Raw Materials Warehouse 1 is a specific destination, while Raw Materials Warehouse is its نوع انبار.

نوع انبار helps the system recognize destinations at the category level, without necessarily limiting every operation to a particular physical warehouse.

## 5. The Relationship Between Action Type, Source, Destination, and Destination Type

The conceptual relationship between these components is as follows:

**The goods' current destination**

The destination to which the goods are currently assigned.

↓

**نوع انبار مبدأ**

The category to which the current destination belongs.

↓

**نوع عملیات**

The operation defined for moving goods from the source warehouse type to the destination warehouse type.

↓

** نوع انبار مقصد**

The category to which the intended destination belongs.

↓

**مقصد مشخص**

The warehouse or location to which the goods are moved.

### Example: Moving Goods from خط تولید to انبار محصول

Suppose there are two destinations named "Production Line 1" and "Production Line 2," both belonging to the نوع انبار "Production Line."

There is also a destination named "Finished Goods Warehouse 1," which belongs to the نوع انبار "Finished Goods Warehouse."

If a نوع عملیات for moving goods from a production line to a finished goods warehouse has been defined for this route, its applicability to the relevant destinations can be evaluated.

However, the suitability of a نوع عملیات cannot be determined solely from the names of the source and destination. The نوع کالا and the conditions defined for that operation may also affect the result.

## 6. What Is نوع کالا, and How Does It Relate to نوع عملیات?

**نوع کالا** is one of the classification attributes used for goods in Silo.

A نوع عملیات may be applicable to one or more specific types of goods. Therefore, not every operation is necessarily suitable for every نوع کالا.

Examples:

- Raw materials → Transfer to the production line
- Semi-finished goods → Transfer to the next production stage
- Finished products → Transfer to the finished goods warehouse

In these examples, نوع کالا helps determine which operation may be suitable for the goods.

### 6.1. Using Product Type to Identify a Suitable Operation

In automated identification processes, different types of goods may pass through the same gate or identification area.

To determine the appropriate operation, the system may consider the identified goods' نوع کالا and compare it with the types of goods permitted for the configured operations.

For example:

- The نوع کالا "Raw Materials" may be associated with an operation for transferring raw materials to the production line.
- The نوع کالا "Finished Product" may be associated with an operation for transferring finished products to the finished goods warehouse.

نوع کالا is one factor in selecting a suitable operation. The final selection must be based on Silo's actual configuration and the conditions of the relevant operation.

## 7. Active Controls for نوع عملیات

Each نوع عملیات may have a set of controls that define the conditions required for its execution.

These controls may determine whether the goods satisfy the requirements for performing the operation.

**An active control does not, by itself, mean that the goods in question have violated its condition.** To determine the outcome, the actual goods data and the conditions associated with the specific operation must be examined.

### 7.1. Controls for Goods Being at the Source

These controls define how to handle tags that are not at the expected source.

- **Ignore tags outside the source (`NotInSourceIgnore`):** Tags that are not at the expected source are ignored so that they do not prevent the operation from continuing.
- **Raise an error for tags outside the source (`NotInSourceError`):** An error is raised if a tag is not at the expected source.

These controls have different behaviors and must not be confused with each other.

For example, a handheld device may identify multiple tags, but not all detected tags necessarily belong to the source expected for the operation.

Depending on the active control, a tag outside the expected source may be ignored or may cause an error.

### 7.2. Controls for Goods Registration

These controls define how to handle tags or serial numbers that have not been registered.

- **Ignore unregistered tags (`NotRegisterIgnore`):** Unregistered tags or serial numbers are ignored.
- **Raise an error for unregistered tags (`NotRegisterError`):** Registration is required. If a tag or serial number is not registered, an error is raised.

An unregistered item is not the same as an item that is not at the expected source. These conditions must be interpreted separately.

### 7.3. Control for Frozen Goods

The `Freezed` control relates to the frozen state of goods when an operation is performed.

If this control is active for a نوع عملیات, the frozen state of the goods must be evaluated according to the rules of that operation.

For example, if the documented rules of an operation prohibit processing frozen goods, that operation must not be considered permissible for such goods.

However, observing that goods are frozen is not sufficient to conclude that all operations are prohibited. The relevant rules for the specific operation must also be checked.

### 7.4. Quality Control Result Controls

Two controls are related to quality control:

- **Goods that failed quality control (`FailedQC`):** Relates to restrictions on operations involving goods that failed quality control.
- **Goods without a quality control result (`NotQc`):** Relates to restrictions on operations involving goods that have no quality control result.

Failing quality control and having no quality control result are different conditions. They must be evaluated according to the rules of the relevant نوع عملیات.

### 7.5. Tag Lifetime Control

The `TagLifeLessThanIgnore` control relates to ignoring tags whose lifetime is below the configured minimum value.

This control differs from controls related to the source, registration, or quality control. Its meaning must not be extended to other conditions.

### 7.6. Document-Related Controls

Some operations may depend on a reference document. In such cases, the document may specify the expected items and quantities.

The documented document-related controls include:

- **Require a document code (`DocCodeSet`):** A document number or code must be provided.
- **Check document items exactly (`DocItemsCheck1`):** The identified items must match the items listed in the document exactly.
- **Check the product and document quantity (`DocItemsCheck2`):** The product code must exist in the document, and the cumulative processed quantity must not exceed the total quantity specified for that product in the document.

The existence of a document does not, by itself, mean that an operation is valid. Document requirements and item matching must be evaluated according to the actual controls enabled for the relevant نوع عملیات.

The exact meaning of `DocItemsCheck3` has not yet been verified. Do not guess its behavior or assign it an unverified meaning.

### 7.7. Principles for Interpreting Controls

Apply the following principles when interpreting controls for a نوع عملیات:

- Interpret each control only according to its documented meaning.
- Do not confuse an active control with a condition that has actually been violated by a particular item.
- Keep controls related to source, registration, frozen status, quality control, and documents distinct.
- Do not attribute unverified behavior to any control.
- If the available information is insufficient to determine the outcome, clearly state the limitation.

## 8. The Relationship Between نوع عملیات and Documents

Some operations are associated with documents. A document may specify which goods and what quantities must be processed during the relevant operation.

For example, when performing a dispatch operation, the identified items may need to be compared with the items listed in the reference document.

In such cases, document-related controls may check whether a document exists, whether an item is included in the document, whether the items match, or whether the permitted quantity has been respected.

These requirements are not the same for every operation. The type and scope of document validation depend on the conditions and controls defined for the specific operation.

## 9. The Relationship Between نوع عملیات and Goods Movement

Performing a نوع عملیات may involve changing the destination assigned to goods. However, performing an operation does not necessarily mean that the goods have physically moved.

For example, an operation may change the status or processing stage of goods without necessarily involving a physical movement.

Therefore, the following concepts must be distinguished:

- Performing an operation
- Changing the recorded destination of goods
- Changing the actual physical location of goods

These concepts are related, but they do not always mean the same thing.

## 10. Integrated Example

Suppose goods classified as "Raw Materials" are located at the destination "Raw Materials Warehouse 1" and need to be transferred to a production line.

In this scenario:

- **Current destination:** Raw Materials Warehouse 1
- **Source نوع انبار:** Raw Materials Warehouse
- **نوع کالا:** Raw Materials
- **نوع عملیات:** The operation defined for transferring raw materials to a production line
- **Destination نوع انبار:** Production Line
- **Final destination:** One of the specific destinations belonging to the Production Line type

To determine whether the operation is suitable, the defined نوع عملیات and its associated conditions must be checked.

For example, the goods may need to be at the expected source, be registered, or satisfy document and quality control requirements. Whether any of these conditions apply depends on the actual controls configured for that نوع عملیات.

This example explains the relationships between the concepts. The existence of such a route or operation in Silo's actual configuration must be verified separately.

## 11. Concept Glossary

| Concept | Definition |
|---|---|
| نوع عملیات | A specific type of operation defined for moving or processing goods |
| انبار or destination | A specific destination to which goods are assigned |
| نوع انبار | The category to which one or more specific destinations belong |
| نوع کالا | A goods classification that may affect the selection of a suitable operation |
| Source | The warehouse type or destination from which an operation begins |
| Final destination | The warehouse type or destination at which an operation ends |
| Active control | A condition or validation configured for a نوع عملیات |
| Reference document | A document that may specify the items, quantities, or other information required for an operation |
| Traceability | The ability to track the operational path and history of goods |

## 12. Accuracy Rules for Answering Questions About These Concepts

When answering questions about نوع عملیات, انبار, نوع انبار, نوع کالا, and controls:

- Distinguish a specific warehouse or destination from its warehouse type.
- Do not assume that every نوع عملیات is applicable to all types of goods.
- Do not assume that every نوع عملیات necessarily requires a document.
- Do not assume that every نوع عملیات necessarily causes physical movement of goods.
- Explain active controls only according to the actual configuration and documented meanings.
- Do not treat an active control alone as proof that an operation has been rejected.
- Do not guess the names, behaviors, or activation conditions of unverified controls.
- If the available information is insufficient for an accurate answer, explicitly state that sufficient documented information is not available.

**Final Principle:** The concepts of نوع عملیات, انبار, نوع انبار, and نوع کالا form the conceptual framework for understanding operations in Silo. The controls associated with each نوع عملیات define the conditions used to evaluate whether that operation can be performed. Every answer must be based on documented concepts and the actual information relevant to the specific case.