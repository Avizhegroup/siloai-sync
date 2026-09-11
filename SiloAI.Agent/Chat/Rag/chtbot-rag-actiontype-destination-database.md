# Silo – Action Type and Destination Agent Knowledge

## 1. Purpose

This document is intended for the Silo AI Agent to understand the internal concepts, relationships, fields, and data structure related to Action Types, Destinations, and their associated operational data.

Use this knowledge to correctly understand relationships when retrieving data, generating queries, or processing supported system operations.

Do not expose internal table names, field names, IDs, or implementation details in normal user-facing responses.

---

## 2. Core Concepts

The main concepts are:

* **Action Type**: Defines an operational process performed on a serialized item.
* **Destination**: Represents the current location state of a serialized item.
* **Destination Type**: Categorizes Destinations.
* **Product Type**: Can determine which Action Types apply to a Product.
* **Active Controls**: Define validations or requirements during an operation.
* **Document Status Rules**: Define allowed document status before and after an operation.
* **Tag Location Behavior**: Determines how location information is handled after an operation.

Conceptual relationship:

**Product Type**
↓
**Action Type**
├── From Destination Type
├── To Destination Type
├── Document Rules
├── Active Controls
└── Tag Location Behavior

A **Destination** belongs to a **Destination Type**.

A serialized item or Tag is associated with a Destination representing its current location or operational state.

---

## 3. Action Type

An Action Type represents a predefined operational workflow.

Each Action Type has a unique identifier:

* `ActionTypeId`

Important conceptual fields include:

* `FromDestinationType`
* `ToDestinationType`
* Product Type
* `PermittedDocStatus`
* `ChangeDocStatus`
* `ChangeTagLocation`
* Active Controls
* RFID-related configuration

An Action Type generally defines an operation between **Destination Types**, not necessarily between specific Destinations.

Therefore, when interpreting an operation:

`FromDestinationType → ActionType → ToDestinationType`

Example:

`انبار محصول → ارسال → انبار فروش‌رفته`

Do not assume that a specific `ActionTypeId` represents a specific business operation unless its mapping is explicitly available.

---

## 4. Destination and Destination Type

A Destination represents the current location or operational state of a serialized item.

A Destination may represent:

* Physical location
* Warehouse
* Production line
* Quarantine area
* Scrap area
* Process stage
* Operational state

Each Destination belongs to a Destination Type.

Conceptually:

`Destination → Destination Type`

Multiple Destinations can belong to the same Destination Type.

Action Types normally operate based on Destination Types rather than individual Destination records.

When querying or validating an operation, distinguish carefully between:

* The specific Destination
* The category or type of that Destination

These are not interchangeable.

---

## 5. Source and Target Relationship

The source and target of an Action Type are represented conceptually by:

* `FromDestinationType`
* `ToDestinationType`

These define the allowed source and target categories for the operation.

When analyzing an operation:

1. Identify the Action Type.
2. Determine its source Destination Type.
3. Determine its target Destination Type.
4. Determine the actual Destination associated with the serialized item when required.
5. Apply configured validation or control rules.

Do not assume that every Destination belonging to a Destination Type is automatically valid for every operation unless supported by the relevant configuration.

---

## 6. Product Type Relationship

An Action Type can be associated with a Product Type.

Conceptually:

`Product → Product Type → Allowed Action Type`

This relationship can be used to determine whether a specific operation is applicable to a Product or serialized item.

Do not assume that every Action Type applies to every Product Type.

---

## 7. Document Status Rules

An Action Type may contain document-related configuration.

### `PermittedDocStatus`

Represents the required or permitted document status before the operation.

When applicable, the document should satisfy this requirement before the operation is processed.

### `ChangeDocStatus`

Represents the document status to apply after a successful operation.

Conceptually:

`Document Current Status → Validate PermittedDocStatus → Perform Operation → ChangeDocStatus`

Do not assume that every Action Type requires a document or changes its status.

---

## 8. Active Controls

An Action Type may have configurable Active Controls.

These controls determine validations or requirements during an operation.

Possible examples include:

* GPS/location validation
* Image capture
* Document matching
* Source filtering
* Other operation-specific controls

When processing or generating logic for an operation, only apply a control when its configuration indicates that it is active.

Do not assume that all controls are enabled for every Action Type.

---

## 9. ChangeTagLocation

`ChangeTagLocation` controls how the Tag's location or Zone information is handled after an operation.

* `True`: The Tag's current Zone/location information is cleared.
* `False`: The existing Zone/location information remains unchanged.

Important distinction:

Changing the operational Destination of an item and changing its physical Zone/location information are not necessarily the same operation.

Do not treat `ChangeTagLocation` as automatically meaning that the Destination does or does not change.

---

## 10. RFID Configuration

An Action Type may include RFID-related configuration.

For example, RFID reader settings such as transmission power may vary depending on the Action Type.

Conceptually:

`Action Type → RFID Configuration`

Do not assume that all operations use the same RFID configuration.

---

## 11. Destination-Level Permissions

A Destination may contain configuration that controls which actions are allowed for Tags or serialized items located there.

Examples include:

* `AllowCancelTag`
* `AllowReprintTag`
* `AllowEditInformation`
* `AllowEditTagValue`

These settings are associated with the Destination and should not automatically be treated as Action Type controls.

Conceptually:

`Destination → Destination-Level Permissions`

while:

`Action Type → Operation-Level Controls`

These represent different levels of configuration.

---

## 12. Operational Relationship Summary

The general operational flow can be understood as:

`Serialized Item / Tag`
↓
`Current Destination`
↓
`Current Destination Type`
↓
`Applicable Action Type`
↓
`FromDestinationType → ToDestinationType`
↓
`Validate Product Type / Document / Active Controls`
↓
`Process Operation`
↓
`Update Destination or Operational State when applicable`
↓
`Apply ChangeDocStatus when applicable`
↓
`Handle Zone/Location based on ChangeTagLocation`

The exact update behavior depends on the configuration and supported workflow.

---

## 13. Important Query and Reasoning Rules

When generating a query or reasoning about these concepts:

1. Do not confuse a Destination with a Destination Type.
2. Do not treat `FromDestinationType` or `ToDestinationType` as specific Destination records.
3. Resolve the Action Type before determining its source and target rules.
4. Consider Product Type restrictions when they are configured.
5. Check document status rules only when the operation uses a document.
6. Apply Active Controls only when enabled.
7. Treat Destination permissions separately from Action Type controls.
8. Do not infer the meaning of an ID without an explicit mapping.
9. Do not assume that every Action Type changes physical location.
10. Do not assume that `ChangeTagLocation` determines Destination changes.
11. Do not assume that every Product Type supports every Action Type.
12. Do not expose internal database terminology in normal user-facing answers.

---

## 14. Accuracy Rules

The Agent must not assume:

* A specific Action Type exists.
* A specific `ActionTypeID` has a known meaning without documented mapping.
* A specific Destination exists.
* A specific Destination Type exists.
* Every operation requires a document.
* Every operation changes document status.
* Every operation changes Destination.
* Every operation changes physical Zone/location.
* Every Active Control is enabled.
* Every Product Type supports every Action Type.
* Every Destination permission is enabled.
* Every Action Type uses the same RFID configuration.

If the required relationship, configuration, or mapping is not available in the provided system knowledge, the Agent must not invent it.
