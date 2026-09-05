# Silo – نوع عملیات and انبار Business Concepts

## 1. What is an نوع عملیات?

*نوع عملیات** in Silo represents a specific type of operation performed on a serialized item.

It defines what kind of operation is being performed, where the item is moving from and to, and what rules or requirements may apply during that operation.

In simple terms:

**نوع عملیات = The definition of an operation and its business rules**

For example, an operation may represent moving a product from «انبار محصول» to «انبار فروش‌رفته».

An نوع عملیات is therefore more than the name of an operation. It can also define the rules, validations, requirements, and expected behavior of that operation.

---

## 2. What Does an نوع عملیات Define?

Depending on its configuration, an نوع عملیات may define:

- The source نوع انبار
- The target نوع انبار
- The type of product allowed in the operation
- Whether a document is required
- Which controls must be applied during the operation
- How the item's location should be handled
- How item identification should be performed
- Other process-specific requirements

The exact behavior depends on the configuration of the نوع عملیات in Silo.

---

## 3. Source and Target in an نوع عملیات

An نوع عملیات represents an operation between a source and a target.

The source and target are generally defined based on **نوع انبار**, rather than a specific physical انبار.

For example:

**انبار محصول → انبار فروش‌رفته**

This means that the نوع عملیات represents an operation that moves or processes an item from a انبار of type «انبار محصول» to a انبار of type «انبار فروش‌رفته».

This approach allows the same type of operation to be used across multiple destinations belonging to the same نوع انبار.

---

## 4. What is a انبار?

A **انبار** represents the current location or operational state of a serialized item in Silo.

A انبار does not necessarily represent a physical location.

It may represent:

- A warehouse
- A production line
- A quarantine area
- A scrap location
- A process stage
- An operational state

Therefore, a انبار can represent either a **physical location** or an **operational/process position**.

---

## 5. What is a نوع انبار?

**نوع انبار** is used to classify Destinations into broader categories.

For example, multiple انبارها may belong to the same نوع انبار:

- انبار محصول
- خط تولید
- انبار ضایعات
- قرنطینه

### Difference Between انبار and نوع انبار

- **انبار** → A specific انبار or location
- **نوع انبار** → The category or type of that انبار

For example:

«انبار شماره ۱» may be a specific انبار, while «انبار محصول» may be its نوع انبار.

---

## 6. Relationship Between نوع عملیات and انبار

The relationship between these concepts can be understood as follows:

**انبار**  
The current location or operational state of the item

↓

**نوع انبار**  
The category of that location or operational state

↓

**نوع عملیات**  
The operation that moves or processes the item between نوع انبار

For example:

**انبار محصول**  
↓  
**نوع عملیات: ارسال کالا**  
↓  
**انبار فروش‌رفته**

The نوع عملیات defines the operation between the source and target نوع انبار.

---

## 7. نوع عملیات and Product Type

An نوع عملیات may be applicable to a specific **Product Type**.

Therefore, not every operation is necessarily applicable to every type of product.

For example:

- مواد اولیه → انتقال به خط تولید
- کالای نیمه‌ساخته → انتقال به مرحله بعدی تولید
- محصول نهایی → انتقال به انبار محصول

The Product Type can therefore be one of the factors used to determine which operation is appropriate for an item.

The exact applicability depends on the configuration of the نوع عملیات.

---

## 8. Controls Applied During an Operation

Some operations may require specific controls or validations when they are performed.

These controls are used to ensure that the operation is performed correctly.

Examples include:

### Location Control

The operation may require the item's location to be checked or location information to be provided.

### Image Control

The operation may require an image to be captured during the operation.

### Document Matching

The identified items may need to be checked against the information in the related document.

### Source Control

The operation may only allow items that are currently located at the expected source to be identified and processed.

The type and activation of these controls depend on the configuration of the operation.

---

## 9. Action Types and Documents

Some operations may be associated with documents.

In such cases:

- The document may need to be in a specific status before the operation can be performed.
- The document status may change after the operation is successfully completed.

For example, a document may move through statuses such as:

**تأیید نشده → تأیید شده → جمع‌آوری شده → ارسال شده**

The document requirements and status changes depend on the configuration of the specific operation.

---

## 10. Item Location Changes

Some operations may change the recorded location of an item in Silo.

For example, when an item is transferred from one location to another, its recorded location may be updated accordingly.

However, not every نوع عملیات necessarily represents a physical movement.

Therefore:

**Performing an نوع عملیات does not necessarily mean that the item's physical location has changed.**

The way the item's location is handled depends on the configuration of the specific operation.

---

## 11. Item Traceability

The relationship between an item, its انبار, and the Action Types performed on it helps Silo maintain operational traceability.

In simple terms:

**Item is at a انبار → an operation is performed → its انبار or operational state may change → the operation history can be tracked.**

This structure helps maintain **Traceability** of serialized items throughout different operational processes.

---

## 12. Destination-Level Permissions

A انبار may have specific permissions that determine which operations or capabilities are allowed there.

For example, depending on the configuration of a انبار:

- Canceling a tag may be allowed.
- Reprinting a tag may be allowed.
- Editing item information may be allowed.
- Editing the value or quantity associated with a tag may be allowed.

These capabilities depend on the configuration of the انبار.

The assistant must not assume that these capabilities are enabled for every انبار.

---

## 13. Simple Example

Suppose an item is currently located in **انبار محصول** and needs to be transferred to **انبار فروش‌رفته**.

In this scenario:

- Current Destination → The item's current location
- Source Destination Type → انبار محصول
- نوع عملیات → The operation used to transfer or send the item
- Target Destination Type → انبار فروش‌رفته

During the operation, Silo may apply additional requirements depending on the نوع عملیات configuration, such as document validation, location checks, image capture, or other controls.

---

## 14. Concepts at a Glance

| Concept | Simple Meaning |
|---|---|
| نوع عملیات | The type of operation performed on an item |
| انبار | The item's current location or operational state |
| نوع انبار | The category or type of a انبار |
| Product Type | The type or category of the product |
| Active Controls | Controls and validations applied during an operation |
| Document | A document associated with an operation |
| Traceability | The ability to track an item's operational path and history |

---

## 15. Important Accuracy Rules

The exact behavior of an نوع عملیات or انبار depends on its configuration in Silo.

The assistant must not assume that:

- A specific نوع عملیات exists.
- A specific operation is available for a specific product.
- A specific انبار exists.
- A specific نوع انبار exists.
- A specific control is enabled.
- An operation necessarily requires a document.
- An operation necessarily changes the item's location.
- A specific operation is available between two destinations.

If the required configuration or business information is not available in the knowledge base, the assistant must clearly state that there is not enough documented information to provide an accurate answer.

The assistant must not provide speculative answers.