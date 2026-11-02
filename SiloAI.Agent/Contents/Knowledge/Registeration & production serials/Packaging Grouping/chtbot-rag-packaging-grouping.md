# Silo Knowledge Base — سطوح بسته‌بندی و گروه‌بندی سریال‌ها

## 1. Document Purpose and Scope

This document describes Silo capabilities in the following area:

- Creating groups and serial packaging levels

This document focuses on capability logic and usage conditions, rather than introducing implemented projects or specific industries.

---

## 2. Packaging Levels and Serial Grouping

### 2.1. Concept of Packaging Levels

In relevant scenarios, Silo supports defining relationships between the serials of individual product units and the serials of higher-level packaging units.

In this model, each packaging level can have its own identifier or serial and can be associated with the components contained within it.

For example, a packaging structure may consist of three levels:

1. **Product Unit:** Each unit has its own independent serial.
2. **Master Package:** Multiple product units are placed inside one package, and the master package may also have its own independent serial.
3. **Pallet:** Multiple master packages are placed on a pallet, and the pallet may also have its own independent serial.

This structure preserves the relationships between the pallet serial, the master package serials, and the individual product unit serials.

### 2.2. Use of Relationships Between Packaging Levels

When relationships between packaging levels have been recorded, the identifier of a higher-level package can be used to access information about the components associated with it, provided that the relevant relationships have been established in the system.

For example, instead of identifying every unit inside a package individually, the package identifier can be read, and the recorded relationship can be used to determine which components belong to it.

This capability is useful in scenarios where tracking the components inside a package, carton, master package, or pallet is important.

### 2.3. Difference Between Serial Grouping and Quantity Management

Packaging levels may serve two different purposes:

**Purpose 1: Tracking Relationships Between Units**

In this case, the serial of a higher-level package is linked to the serials of the components inside it. The objective is to preserve the relationship between the package and its contents.

**Purpose 2: Representing an Aggregated Quantity**

In some scenarios, the primary objective is for one serial to represent a specific quantity of goods. For example, one serial may represent 30 product units, or one serial may represent a pallet containing a number of cartons.

In this case, a distinction must be made between **one serial** and **the number of units covered by that serial**.

These concepts are related but not identical. The existence of a serial alone must not be taken as proof that structural relationships with the independent serials of all individual components have also been recorded.

### 2.4. Relationship to Counting and Inventory Counting

In counting scenarios, the quantity attributed to a serial must correspond to the definition recorded for that serial.

For example:

- A serial representing one product unit represents one unit.
- An aggregated serial defined for 30 units represents 30 units.

Therefore, counting by an aggregated serial must not be treated as counting a single product unit without considering the quantity recorded for that serial.

This section describes the expected logic for aggregated quantities. The implementation details of each counting form must be verified against the capabilities and settings of that specific form.

---

## 3. Guidelines for the Agent When Answering User Questions

### 3.1. Questions About One Serial Representing Multiple Product Units

Explain that the number of serials, the number of units covered by each serial, and the physical quantity of goods are three different concepts.

To provide an accurate answer, determine whether the user is asking about:
- The number of serials
- The number of component units associated with each serial
- The physical quantity expressed as length or weight

---

## 4. Inference Limitations and Rules for Preventing Incorrect Answers

1. The existence of a serial for a package does not, by itself, prove that serial-level relationships with all components inside that package have also been recorded.

2. If behavior depends on the project, version, permissions, or configuration, and reliable information is unavailable, explicitly state this limitation. Do not fill in the missing details by guessing.

---

## 5. Summary of Capabilities

Silo provides various methods for serial issuance, including single-product and multi-product issuance, restricting operations based on a source document, controlling the remaining quantity, configuring the number of units covered by each serial, calculating length and weight in relevant scenarios, and preserving relationships between packaging levels.

When answering users, the agent must always distinguish between existing capabilities, configuration-dependent capabilities, technical limitations, and planned future enhancements. Details that have not been confirmed by the available knowledge must not be presented as established facts.