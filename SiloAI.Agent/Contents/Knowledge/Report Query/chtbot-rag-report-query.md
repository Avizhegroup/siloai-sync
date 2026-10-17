# Application sql server table definitions:

-- ===== Silo database schema | جدول tbl_Destination =====
-- کلیدواژه‌ها: انبار، مقصد، کد انبار، نام انبار
CREATE TABLE tbl_Destination (
    DestinationId int IDENTITY(1,1) NOT NULL,
    DestinationTitle nvarchar(50) NULL,
    DestinationSt int NULL,
    DestinationDesc nvarchar(max) NULL,
    DestinationCode nvarchar(50) NULL,
    DestinationType int NULL,
    DestinationParentId int NULL,
    DestinationParentsId nvarchar(max) NULL,
    DestinationEpc nvarchar(50) NULL
)

-- ===== Silo database schema | جدول tbl_DestinationType =====
-- کلیدواژه‌ها: نوع انبار
CREATE TABLE tbl_DestinationType (
    fld_Id int IDENTITY(1,1) NOT NULL,
    fld_DestinationTypeCode nvarchar(50) NULL,
    fld_DestinationTypeName nvarchar(250) NULL
)

-- ===== Silo database schema | جدول tbl_ProductBrand =====
-- کلیدواژه‌ها: برند
CREATE TABLE tbl_ProductBrand (
    fld_ProductBrandId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    fld_ProductBrandCode nvarchar(128) NOT NULL,
    fld_ProductBrandTitle nvarchar(128) NOT NULL,
    fld_ProductBrandData nvarchar(max) NULL
)

-- ===== Silo database schema | جدول tbl_ProductClass =====
-- کلیدواژه‌ها: طبقه کالا
CREATE TABLE tbl_ProductClass (
    fld_ProductClassId int IDENTITY(1,1) NOT NULL,
    fld_ProductClassCode nvarchar(128) NOT NULL,
    fld_ProductClassTitle nvarchar(256) NOT NULL,
    fld_ProductClassSubTitle nvarchar(512) NULL,
    fld_ProductClassDesc nvarchar(512) NULL
)

-- ===== Silo database schema | جدول tbl_ProductGroup =====
-- کلیدواژه‌ها: گروه کالا، گروه محصول
CREATE TABLE tbl_ProductGroup (
    fld_ProductGroupId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    fld_ProductGroupCode nvarchar(128) NOT NULL,
    fld_ProductGroupTitle nvarchar(128) NOT NULL,
    fld_ProductGroupData nvarchar(max) NULL
)

-- ===== Silo database schema | جدول tbl_ProductPropertyA / B / C =====
-- کلیدواژه‌ها: خط تولید، شیفت کاری، سایز، ویژگی کالا
CREATE TABLE tbl_ProductPropertyA (
    fld_ProductPropertyAId nvarchar(128) NOT NULL,
    fld_ProductPropertyATitle nvarchar(256) NOT NULL,
    fld_ProductPropertyADesc nvarchar(max) NULL,
    fld_ProductPropertyAData nvarchar(max) NOT NULL,
    CONSTRAINT IX_tbl_ProductPropertyATitleUnique UNIQUE (fld_ProductPropertyATitle)
)

CREATE TABLE tbl_ProductPropertyB (
    fld_ProductPropertyBId nvarchar(128) NOT NULL,
    fld_ProductPropertyBTitle nvarchar(512) NOT NULL,
    fld_ProductPropertyBDesc nvarchar(max) NULL,
    fld_ProductPropertyBData nvarchar(max) NOT NULL,
    fld_ProductPropertyAId nvarchar(128) NULL
)

CREATE TABLE tbl_ProductPropertyC (
    fld_ProductPropertyCId nvarchar(128) NOT NULL,
    fld_ProductPropertyCTitle nvarchar(256) NOT NULL,
    fld_ProductPropertyCDesc nvarchar(max) NULL,
    fld_ProductPropertyCData nvarchar(max) NOT NULL,
    fld_ProductPropertyCIdentity int IDENTITY(1,1) NOT NULL,
    fld_ProductPropertyCTemp nvarchar(max) NULL
)

-- ===== Silo database schema | جدول tbl_ProductStatus =====
-- کلیدواژه‌ها: کیفیت، وضعیت کیفی، درجه کیفیت
CREATE TABLE tbl_ProductStatus (
    ProductStatusId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductStatusTitle nvarchar(50) NULL,
    ProductStatusCode nvarchar(50) NULL,
    ProductStatusDesc nvarchar(max) NULL
)

-- ===== Silo database schema | جدول tbl_ProductSubGroup =====
-- کلیدواژه‌ها: زیرگروه کالا
CREATE TABLE tbl_ProductSubGroup (
    fld_ProductSubGroupId int IDENTITY(1,1) NOT NULL,
    fld_ProductSubGroupCode nvarchar(128) NOT NULL,
    fld_ProductSubGroupTitle nvarchar(256) NOT NULL,
    fld_ProductSubGroupSubTitle nvarchar(512) NULL,
    fld_ProductGroupCode nvarchar(128) NOT NULL,
    fld_ProductSubGroupDesc nvarchar(512) NULL
)

-- ===== Silo database schema | جدول tbl_ProductType =====
-- کلیدواژه‌ها: نوع کالا
CREATE TABLE tbl_ProductType (
    ProductTypeId int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductTypeTitle nvarchar(50) NULL,
    ProductTypeParentId nvarchar(10) NULL,
    ProductTypeParentsId nvarchar(max) NULL,
    ProductTypeCode nvarchar(50) NULL
)

-- ===== Silo database schema | جدول tbl_Tags =====
-- کلیدواژه‌ها: موجودی، موجودی کالا، چقدر موجودی داریم، تگ، سریال کالا، EPC، RFID، انبار، ناحیه، تعداد کالا
-- نمونه سؤال: از کد کالای 45266 چقدر موجودی داریم؟ موجودی انبار مرکزی چقدر است؟
CREATE TABLE tbl_Tags (
    Id int IDENTITY(1,1) NOT NULL,
    ProductSerial nvarchar(50) NOT NULL,
    ProductCode nvarchar(50) NULL,
    TagEpc nvarchar(50) NOT NULL,
    ProjectCode nvarchar(50) NULL,
    ProductCount decimal(18,2) NULL,
    ProductName nvarchar(max) NULL,
    ProductType nvarchar(256) NULL,
    ProductStatus nvarchar(256) NULL,
    TagStatus nvarchar(50) NULL DEFAULT (0),
    -- قالب تاریخ شمسی: YYYYMMDDHHMMSS، مانند 14020618131759
    TagRegisterShamsiUnixDate nvarchar(50) NULL,
    TagRegisterUser nvarchar(50) NULL,
    TagTreeParentId int NULL,
    TagTreeSecondParentId int NULL,
    TagTreeParentsId nvarchar(max) NULL,
    NewProductSerial nvarchar(50) NULL,
    ProductProperties nvarchar(max) NULL,
    Lock bit NULL DEFAULT (0),
    Username nvarchar(50) NULL,
    DeviceId nvarchar(50) NULL,
    DeviceIp nvarchar(50) NULL,
    Freeze bit NULL,
    Deactivate bit NULL,
    TagInActionId int NULL,
    TagInDestinationId nvarchar(50) NULL, -- کد انبار
    TagInActionId2 int NULL,
    TagInDestinationId2 nvarchar(50) NULL,
    fld_ProductPropertyAId nvarchar(50) NULL,
    fld_ProductPropertyBId nvarchar(50) NULL,
    fld_ProductPropertyCId nvarchar(50) NULL,
    RegCode nvarchar(50) NULL,
    fld_LastModifierUser nvarchar(128) NULL,
    ContractStatus nvarchar(50) NULL,
    TagZone nvarchar(50) NULL DEFAULT (0),
    TagRegisterDateTime datetime NULL DEFAULT (getdate()),
    ReProduct bit NULL DEFAULT (0),
    fld_InspectActionId int NULL DEFAULT (0),
    fld_LastInspectResult nvarchar(max) NULL,
    fld_ProductGroup nvarchar(128) NULL,
    fld_ProductBrand nvarchar(128) NULL,
    fld_ProductSubGroup nvarchar(128) NULL,
    fld_ProductClass nvarchar(128) NULL,
    TagTreeParentSerial nvarchar(50) NULL,
    TagTreeParentsEpc nvarchar(128) NULL,
    PRIMARY KEY (ProductSerial, TagEpc)
)

-- ===== Silo database schema | جدول tbl_User =====
-- کلیدواژه‌ها: کاربر، اپراتور
CREATE TABLE tbl_User (
    Id nvarchar(128) NOT NULL PRIMARY KEY,
    LastModifiedDate datetime NOT NULL,
    CreateDate datetime NOT NULL,
    IsActive bit NOT NULL,
    CreatorIdentityID nvarchar(128) NOT NULL,
    LastModifierIdentityID nvarchar(128) NULL,
    Name nvarchar(512) NOT NULL,
    Row int IDENTITY(1,1) NOT NULL,
    Email nvarchar(256) NULL,
    EmailConfirmed bit NOT NULL,
    PasswordHash nvarchar(max) NULL,
    SecurityStamp nvarchar(max) NULL,
    PhoneNumber nvarchar(max) NULL,
    PhoneNumberConfirmed bit NOT NULL,
    TwoFactorEnabled bit NOT NULL,
    LockoutEndDateUtc datetime NULL,
    LockoutEnabled bit NOT NULL,
    AccessFailedCount int NOT NULL,
    Username nvarchar(256) NOT NULL,
    Details nvarchar(max) NULL,
    Image nvarchar(50) NULL
)

-- ===== Silo database schema | جدول tbl_Station =====
-- کلیدواژه‌ها: گیت، ایستگاه، RFID
CREATE TABLE tbl_Station (
    fld_StationId int IDENTITY(1,1) NOT NULL,
    fld_StationCode nvarchar(128) NULL,
    fld_StationName nvarchar(512) NULL,
    fld_StationType int NULL,
    fld_StationActionType int NULL,
    fld_StationStatus int NULL,
    fld_StationReaders nvarchar(max) NULL,
    fld_StationDescription nvarchar(1024) NULL,
    fld_StationSettings nvarchar(max) NULL,
    fld_StationFromDestination nvarchar(50) NULL,
    fld_StationToDestination nvarchar(50) NULL,
    fld_StationMacAddress nvarchar(50) NULL
)

-- ===== Silo database schema | جدول tbl_ActionTypes =====
-- کلیدواژه‌ها: نوع عملیات، عنوان عملیات
CREATE TABLE tbl_ActionTypes (
    Id int IDENTITY(1,1) NOT NULL,
    fld_ActionTypeId int NULL,
    fld_ActionTypeFromDestinationType nvarchar(50) NULL,
    fld_ActionTypeToTypeDestinationType nvarchar(50) NULL,
    fld_ActionTypeTitle nvarchar(50) NULL,
    fld_ActionTypePermitedDocStatus nvarchar(50) NULL,
    fld_ActionTypeChangeDocStatus nvarchar(50) NULL,
    fld_ActionTypeActiveControls nvarchar(max) NULL,
    fld_ActionTypeRfidPower int NULL,
    fld_ActionTypeChangeTagLocation int NULL,
    fld_ActionTypeProductType nvarchar(50) NULL
)

-- ===== Silo database schema | جدول tbl_TagsMovement =====
-- کلیدواژه‌ها: ورود، خروج، برگشت، سابقه، تاریخچه، حرکت کالا، سریال، تگ
-- نمونه سؤال: سابقه ورود و خروج کالا چیست؟ تاریخچه تگ را بده.
CREATE TABLE tbl_TagsMovement (
    TagsMovementId int IDENTITY(1,1) NOT NULL,
    TagEpc nvarchar(50) NULL,
    ProductCode nvarchar(50) NULL,
    ProductSerial nvarchar(50) NULL,
    ProductCount decimal(18,2) NULL,
    HMovementActionId int NULL,
    HTagsMovementDate nvarchar(10) NULL,
    HTagsMovementTime nvarchar(5) NULL,
    HTagsMovementDateTime datetime NULL,
    HTagsMovementSt int NULL DEFAULT (0),
    RMovementActionId int NULL,
    RTagsMovementDate nvarchar(10) NULL,
    RTagsMovementTime nvarchar(5) NULL,
    RTagsMovementDateTime datetime NULL,
    RTagsMovementSt int NULL,
    MovementData nvarchar(max) NULL,
    ApiSendStatus int NULL DEFAULT (0),
    ApiSendUser nvarchar(128) NULL,
    ApiSendDateTime datetime NULL,
    ApiSendData nvarchar(max) NULL,
    RMovementActionType int NULL,
    RMovementActionDocumentId nvarchar(512) NULL,
    RMovementActionUHFLogId nvarchar(50) NULL,
    RMovementActionDestinationId nvarchar(50) NULL
)

-- ===== Silo database schema | جدول tbl_MovementActions =====
-- کلیدواژه‌ها: عملیات، سند، اپراتور، گیت، پلاک، راننده، تاریخ عملیات، زمان عملیات
-- نمونه سؤال: عملیات‌های امروز چیست؟ چه عملیاتی ثبت شده؟
CREATE TABLE tbl_MovementActions (
    MovementActionId int NULL,
    MovementActionTp int NULL,
    MovementActionStore nvarchar(50) NULL,
    MovementActionUserId nvarchar(max) NULL,
    MovementActionDate nvarchar(10) NULL,
    MovementActionTime nvarchar(5) NULL,
    MovementActionDateTime datetime NULL,
    MovementActionCountTags int NULL,
    MovementActionDestinationId nvarchar(50) NULL,
    MovementActionCarPlaque nvarchar(16) NULL,
    MovementActionDriverName nvarchar(50) NULL,
    MovementActionDriverMobile nvarchar(50) NULL,
    MovementActionData nvarchar(max) NULL,
    MovementActionUHFLogId nvarchar(50) NULL,
    MovementActionLinkId int NULL,
    MovementActionLinkDestId nvarchar(50) NULL,
    MovementActionTruckCrossId bigint NULL DEFAULT (0),
    MovementActionDesc nvarchar(max) NULL,
    MovementActionDocumentId nvarchar(512) NULL,
    MovementActionUHFLogGate nvarchar(30) NULL
)

-- ===== Silo database schema | جدول tbl_Products =====
-- کلیدواژه‌ها: کالا، محصول، کد کالا، کدکالا، نام کالا، نام محصول، مشخصات کالا، کد فنی، اطلاعات پایه کالا
-- نمونه سؤال: کد کالای 45266 مربوط به چه محصولی است؟ مشخصات کالا را بده.
CREATE TABLE tbl_Products(
	Id int IDENTITY(1,1) NOT NULL,
	ProductCode nvarchar(50) NOT NULL,
    ProductTitle nvarchar(250) NULL,
	ProductENTitle nvarchar(250) NULL,
	ProductPackValue decimal(18, 2) NULL,
	ProductPackWeight decimal(18, 2) NULL,
	ProductPackVolume decimal(18, 2) NULL,
	ProductCountInPack decimal(18, 2) NULL,
	ProductValue decimal(18, 2) NULL,
	ProductTechnicalCode nvarchar(50) NULL,
	ProductProperties nvarchar(max) NULL,
	ProductType nvarchar(50) NULL,
	ProductStatus nvarchar(50) NULL,
	ProductSize nvarchar(50) NULL,
	ProductUnit nvarchar(50) NULL,
	ProductRegUser nvarchar(50) NULL,
	ProductRegDateTime datetime NULL,
	ProductImageName nvarchar(50) NULL,
	ProductTechnicalData nvarchar(max) NULL,
	fld_ProductGroup nvarchar(128) NULL,
	fld_ProductBrand nvarchar(128) NULL,
	fld_ProductGuaranteeType int NULL,
	fld_ProductGuaranteeDays int NULL,
	fld_ProductExpireType int NULL,
	fld_ProductExpireDays int NULL,
	fld_ProductIsActive bit NULL,
	fld_ProductSubGroup nvarchar(128) NULL,
	fld_ProductClass nvarchar(128) NULL,
	fld_ProductGuaranteeEndDate nvarchar(10) NULL,
	fld_ProductExpireEndDate nvarchar(10) NULL,
	ProductGalleryId int NULL,
	fld_ProductGuaranteeMonths int NULL,
	fld_ProductExpireMonths int NULL,
	fld_HasDoubleTag int NULL
)

-- ===== Silo database schema | جدول tbl_Zones =====
-- کلیدواژه‌ها: ناحیه، محل نگهداری، جانمایی، ظرفیت، راهرو
CREATE TABLE tbl_Zones (
    id int IDENTITY(1,1) NOT NULL,
    ZoneCode nvarchar(50) NULL,
    ZoneTitle nvarchar(50) NULL,
    ZoneCapacity decimal(18,2) NULL,
    ZoneDimention nvarchar(50) NULL,
    ZoneParentCode nvarchar(50) NULL,
    ZoneParentLayer int NULL,
    ZoneStoreCode nvarchar(50) NULL, -- کد انبار
    ZoneCountPixle int NULL,
    ZoneOccupiedCapacity int NULL DEFAULT (0),
    MinZoneCapacity decimal(18,2) NULL,
    MaxZoneCapacity decimal(18,2) NULL,
    ZoneRowIndex int NULL DEFAULT (0),
    ZoneType int NULL,
    ZoneAddress nvarchar(250) NULL,
    ZoneCorridorId int NULL DEFAULT (0),
    ZoneMapXPosition int NULL,
    ZoneMapYPosition int NULL
)
