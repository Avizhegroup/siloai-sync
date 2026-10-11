# Customer customization document for terminology: Sazb-Khodro Project

## 1. Purpose

This document defines the names used for product concepts in this project and
the form that exists for each of them. It supplements the general Silo
knowledge base. General documents use standard Silo names; this project uses
the names below for the same concepts. Only the names differ; the meaning and
functionality of each concept are the same.

## 2. Terminology Table

| Name used in this project | Deprecated name (never shown to users) |
|---|---|
| کاربری | نوع کالا |
| نوع سوخت | سایز |
| برند | برند |
| سیستم | گروه (گروه کالا) |
| محور | کیفیت (درجه کیفیت) |
| تیپ | زیرگروه (زیرگروه کالا) |
| طبقه | طبقه (طبقه کالا) |
| خودرو | کالا |
| افزودن مدل خودرو | افزودن کالا |

## 3. Phrase-Level Equivalents

| Deprecated phrase (never shown to users) | Phrase to use in answers |
|---|---|
| تعریف گروه کالا / افزودن گروه کالا | افزودن سیستم |
| مدیریت زیرگروه کالا / افزودن زیرگروه کالا | افزودن تیپ |
| نوع کالا | کاربری |
| سایز | نوع سوخت |
| درجه کیفیت | محور |
| گروه کالا → زیرگروه کالا | سیستم → تیپ |
| فرم افزودن کالا | فرم افزودن مدل خودرو |

## 4. Forms

Each concept has its own separate form:

| Concept | Form |
|---|---|
| کاربری | افزودن کاربری |
| نوع سوخت | افزودن نوع سوخت |
| برند | افزودن برند |
| سیستم | افزودن سیستم |
| محور | افزودن محور |
| تیپ | افزودن تیپ |
| طبقه | افزودن طبقه |
| خودرو | افزودن مدل خودرو |

Each form adds only its own concept. Forms are not interchangeable:
افزودن سیستم does not add a تیپ, and افزودن تیپ does not add a سیستم.

## 5. Concepts

### سیستم
سیستم is the main product category used to organize cars into primary groups.
A تیپ always belongs to a سیستم.
Form: افزودن سیستم.

### تیپ
تیپ is a more detailed category inside a سیستم. When defining a تیپ, the
related سیستم is specified.
Relationship: **سیستم → تیپ**.
Form: افزودن تیپ.

### کاربری
کاربری identifies the general nature of the product.
Form: افزودن کاربری.

### نوع سوخت
نوع سوخت manages code, title, description, units, weight, volume, and other
physical characteristics.
Form: افزودن نوع سوخت.

### برند
برند identifies the commercial brand. Main information: brand code and title.
Form: افزودن برند.

### محور
محور specifies the quality status of the product. Main information: code,
title, and description of the quality standard or status.
Form: افزودن محور.

### طبقه
طبقه is the structural and specialized classification of products.
Main information: code, title, subtitle, description.
Form: افزودن طبقه.

### خودرو (افزودن مدل خودرو)
خودرو is the item managed in the system. Registering a new car, adding a car,
editing a car, and viewing car information are all done in the افزودن مدل خودرو form.
Related wording: ثبت خودرو جدید، افزودن خودرو، ثبت مدل خودرو، ویرایش خودرو.

---

## 6. Documentation and Terminology Update Status

**Status: Completed**

The customer-specific terminology definitions and terminology replacement rules for the Sazb-Khodro Project have been completed.

All terminology mappings, phrase-level equivalents, form names, and documented relationships defined in this document are the authoritative naming rules for this project.

When using the general Silo knowledge base for this project, always apply the latest terminology definitions in this document.

If this document is updated in the future, the updated terminology and form names must replace the previous names wherever applicable, including in explanations, examples, tables, procedures, and user-facing responses.

Only terminology and naming are changed. The underlying business concepts, functionality, relationships, operational procedures, and factual content must remain unchanged unless an explicit business-rule update is provided.

Do not continue using outdated terminology when a newer name is defined in this document.