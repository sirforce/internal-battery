// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Note: this file is purely for documentation. Any contents are not expected
// to be loaded as the JS file.

/**
 * Message representing a period of time between two timestamps.
 *
 * @property {Object} startTime
 *   Begin of the period (inclusive).
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} endTime
 *   End of the period (exclusive).
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @typedef TimestampRange
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.TimestampRange definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const TimestampRange = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * RecruitingNote represents a note/comment regarding the recruiting for a
 * candidate. For example, "This candidate is a potential match for a frontend
 * engineer at SF".
 *
 * @property {string} note
 *   Optional.
 *
 *   The content of note.
 *
 *   Number of characters allowed is 4,000.
 *
 * @property {string} recruiter
 *   Optional.
 *
 *   The resource name of the person who wrote the notes, such as
 *   "projects/api-test-project/tenants/foo/recruiters/bar".
 *
 * @property {Object} createDate
 *   Optional.
 *
 *   The create date of the note.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} type
 *   Optional.
 *
 *   The note type.
 *
 *   Number of characters allowed is 100.
 *
 * @typedef RecruitingNote
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.RecruitingNote definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const RecruitingNote = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Output only. A resource that represents a location with full geographic
 * information.
 *
 * @property {number} locationType
 *   The type of a location, which corresponds to the address lines field of
 *   google.type.PostalAddress. For example, "Downtown, Atlanta, GA, USA"
 *   has a type of LocationType.NEIGHBORHOOD, and "Kansas City, KS, USA"
 *   has a type of LocationType.LOCALITY.
 *
 *   The number should be among the values of [LocationType]{@link google.cloud.talent.v4beta1.LocationType}
 *
 * @property {Object} postalAddress
 *   Postal address of the location that includes human readable information,
 *   such as postal delivery and payments addresses. Given a postal address,
 *   a postal service can deliver items to a premises, P.O. Box, or other
 *   delivery location.
 *
 *   This object should have the same structure as [PostalAddress]{@link google.type.PostalAddress}
 *
 * @property {Object} latLng
 *   An object representing a latitude/longitude pair.
 *
 *   This object should have the same structure as [LatLng]{@link google.type.LatLng}
 *
 * @property {number} radiusMiles
 *   Radius in miles of the job location. This value is derived from the
 *   location bounding box in which a circle with the specified radius
 *   centered from google.type.LatLng covers the area associated with the
 *   job location.
 *   For example, currently, "Mountain View, CA, USA" has a radius of
 *   6.17 miles.
 *
 * @typedef Location
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Location definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Location = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * An enum which represents the type of a location.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  LocationType: {

    /**
     * Default value if the type isn't specified.
     */
    LOCATION_TYPE_UNSPECIFIED: 0,

    /**
     * A country level location.
     */
    COUNTRY: 1,

    /**
     * A state or equivalent level location.
     */
    ADMINISTRATIVE_AREA: 2,

    /**
     * A county or equivalent level location.
     */
    SUB_ADMINISTRATIVE_AREA: 3,

    /**
     * A city or equivalent level location.
     */
    LOCALITY: 4,

    /**
     * A postal code level location.
     */
    POSTAL_CODE: 5,

    /**
     * A sublocality is a subdivision of a locality, for example a city borough,
     * ward, or arrondissement. Sublocalities are usually recognized by a local
     * political authority. For example, Manhattan and Brooklyn are recognized
     * as boroughs by the City of New York, and are therefore modeled as
     * sublocalities.
     */
    SUB_LOCALITY: 6,

    /**
     * A district or equivalent level location.
     */
    SUB_LOCALITY_1: 7,

    /**
     * A smaller district or equivalent level display.
     */
    SUB_LOCALITY_2: 8,

    /**
     * A neighborhood level location.
     */
    NEIGHBORHOOD: 9,

    /**
     * A street address level location.
     */
    STREET_ADDRESS: 10
  }
};

/**
 * Input only.
 *
 * Meta information related to the job searcher or entity
 * conducting the job search. This information is used to improve the
 * performance of the service.
 *
 * @property {string} domain
 *   Required if allow_missing_ids is unset or `false`.
 *
 *   The client-defined scope or source of the service call, which typically
 *   is the domain on
 *   which the service has been implemented and is currently being run.
 *
 *   For example, if the service is being run by client <em>Foo, Inc.</em>, on
 *   job board www.foo.com and career site www.bar.com, then this field is
 *   set to "foo.com" for use on the job board, and "bar.com" for use on the
 *   career site.
 *
 *   Note that any improvements to the model for a particular tenant site rely
 *   on this field being set correctly to a unique domain.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {string} sessionId
 *   Required if allow_missing_ids is unset or `false`.
 *
 *   A unique session identification string. A session is defined as the
 *   duration of an end user's interaction with the service over a certain
 *   period.
 *   Obfuscate this field for privacy concerns before
 *   providing it to the service.
 *
 *   Note that any improvements to the model for a particular tenant site rely
 *   on this field being set correctly to a unique session ID.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {string} userId
 *   Required if allow_missing_ids is unset or `false`.
 *
 *   A unique user identification string, as determined by the client.
 *   To have the strongest positive impact on search quality
 *   make sure the client-level is unique.
 *   Obfuscate this field for privacy concerns before
 *   providing it to the service.
 *
 *   Note that any improvements to the model for a particular tenant site rely
 *   on this field being set correctly to a unique user ID.
 *
 *   The maximum number of allowed characters is 255.
 *
 * @property {boolean} allowMissingIds
 *   Optional.
 *
 *   If set to `true`, domain, session_id and user_id are optional.
 *   Only set when any of these fields isn't available for some reason. It
 *   is highly recommended not to set this field and provide accurate
 *   domain, session_id and user_id for the best service experience.
 *
 * @property {Object} deviceInfo
 *   Optional.
 *
 *   The type of device used by the job seeker at the time of the call to the
 *   service.
 *
 *   This object should have the same structure as [DeviceInfo]{@link google.cloud.talent.v4beta1.DeviceInfo}
 *
 * @typedef RequestMetadata
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.RequestMetadata definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const RequestMetadata = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Output only. Additional information returned to client, such as debugging
 * information.
 *
 * @property {string} requestId
 *   A unique id associated with this call.
 *   This id is logged for tracking purposes.
 *
 * @typedef ResponseMetadata
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.ResponseMetadata definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const ResponseMetadata = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Device information collected from the job seeker, candidate, or
 * other entity conducting the job search. Providing this information improves
 * the quality of the search results across devices.
 *
 * @property {number} deviceType
 *   Optional.
 *
 *   Type of the device.
 *
 *   The number should be among the values of [DeviceType]{@link google.cloud.talent.v4beta1.DeviceType}
 *
 * @property {string} id
 *   Optional.
 *
 *   A device-specific ID. The ID must be a unique identifier that
 *   distinguishes the device from other devices.
 *
 * @typedef DeviceInfo
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.DeviceInfo definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const DeviceInfo = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * An enumeration describing an API access portal and exposure mechanism.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  DeviceType: {

    /**
     * The device type isn't specified.
     */
    DEVICE_TYPE_UNSPECIFIED: 0,

    /**
     * A desktop web browser, such as, Chrome, Firefox, Safari, or Internet
     * Explorer)
     */
    WEB: 1,

    /**
     * A mobile device web browser, such as a phone or tablet with a Chrome
     * browser.
     */
    MOBILE_WEB: 2,

    /**
     * An Android device native application.
     */
    ANDROID: 3,

    /**
     * An iOS device native application.
     */
    IOS: 4,

    /**
     * A bot, as opposed to a device operated by human beings, such as a web
     * crawler.
     */
    BOT: 5,

    /**
     * Other devices types.
     */
    OTHER: 6
  }
};

/**
 * Custom attribute values that are either filterable or non-filterable.
 *
 * @property {string[]} stringValues
 *   Optional but exactly one of string_values or long_values must
 *   be specified.
 *
 *   This field is used to perform a string match (`CASE_SENSITIVE_MATCH` or
 *   `CASE_INSENSITIVE_MATCH`) search.
 *   For filterable `string_value`s, a maximum total number of 200 values
 *   is allowed, with each `string_value` has a byte size of no more than
 *   255B. For unfilterable `string_values`, the maximum total byte size of
 *   unfilterable `string_values` is 50KB.
 *
 *   Empty string isn't allowed.
 *
 * @property {number[]} longValues
 *   Optional but exactly one of string_values or long_values must
 *   be specified.
 *
 *   This field is used to perform number range search.
 *   (`EQ`, `GT`, `GE`, `LE`, `LT`) over filterable `long_value`.
 *
 *   Currently at most 1 long_values is supported.
 *
 * @property {boolean} filterable
 *   Optional.
 *
 *   If the `filterable` flag is true, custom field values are searchable.
 *   If false, values are not searchable.
 *
 *   Default is false.
 *
 * @typedef CustomAttribute
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.CustomAttribute definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const CustomAttribute = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Output only. Spell check result.
 *
 * @property {boolean} corrected
 *   Indicates if the query was corrected by the spell checker.
 *
 * @property {string} correctedText
 *   Correction output consisting of the corrected keyword string.
 *
 * @property {string} correctedHtml
 *   Corrected output with html tags to highlight the corrected words.
 *   Corrected words are called out with the "<b><i>...</i></b>" html tags.
 *
 *   For example, the user input query is "software enginear", where the second
 *   word, "enginear," is incorrect. It should be "engineer". When spelling
 *   correction is enabled, this value is
 *   "software <b><i>engineer</i></b>".
 *
 * @typedef SpellingCorrection
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.SpellingCorrection definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const SpellingCorrection = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Job compensation details.
 *
 * @property {Object[]} entries
 *   Optional.
 *
 *   Job compensation information.
 *
 *   At most one entry can be of type
 *   CompensationInfo.CompensationType.BASE, which is
 *   referred as **base compensation entry** for the job.
 *
 *   This object should have the same structure as [CompensationEntry]{@link google.cloud.talent.v4beta1.CompensationEntry}
 *
 * @property {Object} annualizedBaseCompensationRange
 *   Output only. Annualized base compensation range. Computed as
 *   base compensation entry's CompensationEntry.amount times
 *   CompensationEntry.expected_units_per_year.
 *
 *   See CompensationEntry for explanation on compensation annualization.
 *
 *   This object should have the same structure as [CompensationRange]{@link google.cloud.talent.v4beta1.CompensationRange}
 *
 * @property {Object} annualizedTotalCompensationRange
 *   Output only. Annualized total compensation range. Computed as
 *   all compensation entries' CompensationEntry.amount times
 *   CompensationEntry.expected_units_per_year.
 *
 *   See CompensationEntry for explanation on compensation annualization.
 *
 *   This object should have the same structure as [CompensationRange]{@link google.cloud.talent.v4beta1.CompensationRange}
 *
 * @typedef CompensationInfo
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.CompensationInfo definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const CompensationInfo = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * A compensation entry that represents one component of compensation, such
   * as base pay, bonus, or other compensation type.
   *
   * Annualization: One compensation entry can be annualized if
   * - it contains valid amount or range.
   * - and its expected_units_per_year is set or can be derived.
   * Its annualized range is determined as (amount or range) times
   * expected_units_per_year.
   *
   * @property {number} type
   *   Optional.
   *
   *   Compensation type.
   *
   *   Default is CompensationType.COMPENSATION_TYPE_UNSPECIFIED.
   *
   *   The number should be among the values of [CompensationType]{@link google.cloud.talent.v4beta1.CompensationType}
   *
   * @property {number} unit
   *   Optional.
   *
   *   Frequency of the specified amount.
   *
   *   Default is CompensationUnit.COMPENSATION_UNIT_UNSPECIFIED.
   *
   *   The number should be among the values of [CompensationUnit]{@link google.cloud.talent.v4beta1.CompensationUnit}
   *
   * @property {Object} amount
   *   Optional.
   *
   *   Compensation amount.
   *
   *   This object should have the same structure as [Money]{@link google.type.Money}
   *
   * @property {Object} range
   *   Optional.
   *
   *   Compensation range.
   *
   *   This object should have the same structure as [CompensationRange]{@link google.cloud.talent.v4beta1.CompensationRange}
   *
   * @property {string} description
   *   Optional.
   *
   *   Compensation description.  For example, could
   *   indicate equity terms or provide additional context to an estimated
   *   bonus.
   *
   * @property {Object} expectedUnitsPerYear
   *   Optional.
   *
   *   Expected number of units paid each year. If not specified, when
   *   Job.employment_types is FULLTIME, a default value is inferred
   *   based on unit. Default values:
   *   - HOURLY: 2080
   *   - DAILY: 260
   *   - WEEKLY: 52
   *   - MONTHLY: 12
   *   - ANNUAL: 1
   *
   *   This object should have the same structure as [DoubleValue]{@link google.protobuf.DoubleValue}
   *
   * @typedef CompensationEntry
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.CompensationInfo.CompensationEntry definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
   */
  CompensationEntry: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * Compensation range.
   *
   * @property {Object} maxCompensation
   *   Optional.
   *
   *   The maximum amount of compensation. If left empty, the value is set
   *   to a maximal compensation value and the currency code is set to
   *   match the currency code of
   *   min_compensation.
   *
   *   This object should have the same structure as [Money]{@link google.type.Money}
   *
   * @property {Object} minCompensation
   *   Optional.
   *
   *   The minimum amount of compensation. If left empty, the value is set
   *   to zero and the currency code is set to match the
   *   currency code of max_compensation.
   *
   *   This object should have the same structure as [Money]{@link google.type.Money}
   *
   * @typedef CompensationRange
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.CompensationInfo.CompensationRange definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
   */
  CompensationRange: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * The type of compensation.
   *
   * For compensation amounts specified in non-monetary amounts,
   * describe the compensation scheme in the CompensationEntry.description.
   *
   * For example, tipping format is described in
   * CompensationEntry.description (for example, "expect 15-20% tips based
   * on customer bill.") and an estimate of the tips provided in
   * CompensationEntry.amount or CompensationEntry.range ($10 per hour).
   *
   * For example, equity is described in CompensationEntry.description
   * (for example, "1% - 2% equity vesting over 4 years, 1 year cliff") and
   * value estimated in CompensationEntry.amount or
   * CompensationEntry.range. If no value estimate is possible, units are
   * CompensationUnit.COMPENSATION_UNIT_UNSPECIFIED and then further
   * clarified in CompensationEntry.description field.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  CompensationType: {

    /**
     * Default value.
     */
    COMPENSATION_TYPE_UNSPECIFIED: 0,

    /**
     * Base compensation: Refers to the fixed amount of money paid to an
     * employee by an employer in return for work performed. Base compensation
     * does not include benefits, bonuses or any other potential compensation
     * from an employer.
     */
    BASE: 1,

    /**
     * Bonus.
     */
    BONUS: 2,

    /**
     * Signing bonus.
     */
    SIGNING_BONUS: 3,

    /**
     * Equity.
     */
    EQUITY: 4,

    /**
     * Profit sharing.
     */
    PROFIT_SHARING: 5,

    /**
     * Commission.
     */
    COMMISSIONS: 6,

    /**
     * Tips.
     */
    TIPS: 7,

    /**
     * Other compensation type.
     */
    OTHER_COMPENSATION_TYPE: 8
  },

  /**
   * Pay frequency.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  CompensationUnit: {

    /**
     * Default value.
     */
    COMPENSATION_UNIT_UNSPECIFIED: 0,

    /**
     * Hourly.
     */
    HOURLY: 1,

    /**
     * Daily.
     */
    DAILY: 2,

    /**
     * Weekly
     */
    WEEKLY: 3,

    /**
     * Monthly.
     */
    MONTHLY: 4,

    /**
     * Yearly.
     */
    YEARLY: 5,

    /**
     * One time.
     */
    ONE_TIME: 6,

    /**
     * Other compensation units.
     */
    OTHER_COMPENSATION_UNIT: 7
  }
};

/**
 * Resource that represents a license or certification.
 *
 * @property {string} displayName
 *   Optional.
 *
 *   Name of license or certification.
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} acquireDate
 *   Optional.
 *
 *   Acquisition date or effective date of license or certification.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} expireDate
 *   Optional.
 *
 *   Expiration date of license of certification.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {string} authority
 *   Optional.
 *
 *   Authority of license, such as government.
 *
 *   Number of characters allowed is 100.
 *
 * @property {string} description
 *   Optional.
 *
 *   Description of license or certification.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {Object} current
 *   Optional.
 *
 *   Indicates if it's the person's current certification.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @typedef Certification
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Certification definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Certification = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that that represents an experience.
 *
 * @property {string} displayName
 *   Required.
 *
 *   The name of the experience the person has.
 *
 * @property {number} experienceYears
 *   Optional.
 *
 *   The years of experience in this specific experience.
 *
 * @typedef Experience
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Experience definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Experience = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a skill of a candidate.
 *
 * @property {string} displayName
 *   Optional.
 *
 *   Skill display name.
 *
 *   For example, "Java", "Python".
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} lastUsedDate
 *   Optional.
 *
 *   The last time this skill was used.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {number} level
 *   Optional.
 *
 *   Skill proficiency level which indicates how proficient the candidate is at
 *   this skill.
 *
 *   The number should be among the values of [SkillProficiencyLevel]{@link google.cloud.talent.v4beta1.SkillProficiencyLevel}
 *
 * @property {string} context
 *   Optional.
 *
 *   A paragraph describes context of this skill.
 *
 *   Number of characters allowed is 100,000.
 *
 * @property {number} experienceYears
 *   Optional.
 *
 *   Years of experience with the skill.
 *
 * @property {Object} canBeTested
 *   Optional.
 *
 *   Whether the person is willing to be tested on their proficiency in this
 *   skill.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {string} skillNameSnippet
 *   Output only. Skill name snippet shows how the display_name is related
 *   to a search query. It's empty if the display_name isn't related to the
 *   search query.
 *
 * @typedef Skill
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Skill definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Skill = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Resource that represents a skill of a candidate.
 *
 * @property {string} displayName
 *   Optional.
 *
 *   Skill display name.
 *
 *   For example, "English", "Spanish".
 *
 *   Number of characters allowed is 100.
 *
 * @property {Object} lastUsedDate
 *   Optional.
 *
 *   The last time this skill was used.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {number} level
 *   Optional.
 *
 *   Skill proficiency level which indicates how proficient the candidate is at
 *   this skill.
 *
 *   The number should be among the values of [SkillProficiencyLevel]{@link google.cloud.talent.v4beta1.SkillProficiencyLevel}
 *
 * @property {number[]} contexts
 *   Optional.
 *
 *   A paragraph describes context of this skill.
 *
 *   The number should be among the values of [LanguageFluencyContext]{@link google.cloud.talent.v4beta1.LanguageFluencyContext}
 *
 * @property {number} experienceYears
 *   Optional.
 *
 *   Years of experience with the skill.
 *
 * @property {Object} canBeTested
 *   Optional.
 *
 *   Whether the person is willing to be tested on their proficiency in this
 *   skill.
 *
 *   This object should have the same structure as [BoolValue]{@link google.protobuf.BoolValue}
 *
 * @property {string} skillNameSnippet
 *   Output only. Skill name snippet shows how the display_name is related
 *   to a search query. It's empty if the display_name isn't related to the
 *   search query.
 *
 * @typedef LanguageFluency
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.LanguageFluency definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const LanguageFluency = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Enum that represents the skill proficiency level.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  LanguageFluencyContext: {

    /**
     * Default value.
     */
    LANGUAGE_FLUENCY_CONTEXT_UNSPECIFIED: 0,

    /**
     * Skill is in writing the language.
     */
    WRITING: 1,

    /**
     * Skill is in reading the language.
     */
    READING: 2,

    /**
     * Skill is in speaking the language.
     */
    SPEAKING: 3,

    /**
     * Another language skill contexte.
     */
    OTHER_LANGUAGE_FLUENCY_CONTEXT: 4
  }
};

/**
 * Time segment details. This is used to represent a candidate's available
 * contact time, working hours of a job, and so on.
 *
 * @property {boolean} available
 *   This time segment is available for scheduling if true.
 *
 * @property {number[]} dayOfWeek
 *   The day of the week this time segment is for. Generally used for
 *   repeating availability events, like work scheduling.
 *
 *   The number should be among the values of [DayOfWeek]{@link google.type.DayOfWeek}
 *
 * @property {Object} startDate
 *   The date (inclusive) this time segment starts from. Generally used for
 *   one-time availability events, like scheduling calls or interviews.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} endDate
 *   The date (inclusive) this time segment ends on. Generally used for
 *   one-time availability events, like scheduling calls or interviews.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} startTime
 *   The time of day (inclusive) this time segment starts from.
 *
 *   This object should have the same structure as [TimeOfDay]{@link google.type.TimeOfDay}
 *
 * @property {Object} endTime
 *   The time of day (exclusive) this time segment ends on.
 *
 *   This object should have the same structure as [TimeOfDay]{@link google.type.TimeOfDay}
 *
 * @property {string[]} shifts
 *   The shifts this time segment is available for, for example "3rd shift".
 *
 * @property {string} notes
 *   Additional notes about this time segment.
 *
 * @typedef TimeSegment
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.TimeSegment definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const TimeSegment = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Details of a reference check.
 *
 * @property {Object} checkTime
 *   Required.
 *
 *   Customer provided timestamp of when the reference check was done took
 *   place.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} referencePersonId
 *   Optional.
 *
 *   Unique identifier of the person who has provided the reference data.
 *
 * @property {string} relationship
 *   Required.
 *
 *   What is the relationship of the candidate to the reference?
 *
 * @property {string} notes
 *   Notes captured by the recruiter while checking the reference.
 *
 * @property {number} outcome
 *   Required.
 *
 *   The overall outcome resulting from this reference (passed, failed,
 *   nuetral / inconclusive).
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @typedef Reference
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Reference definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Reference = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Details of an assessment.
 *
 * @property {number} assessmentType
 *   Required.
 *
 *   What type of assessment is this (for example, skill, certification)?
 *
 *   The number should be among the values of [AssessmentType]{@link google.cloud.talent.v4beta1.AssessmentType}
 *
 * @property {string} displayName
 *   Optional.
 *
 *   The name of the assessment (typically from a list of values used by the
 *   tenant e.g., test names, and so on).
 *
 * @property {Object} assessmentTime
 *   Required.
 *
 *   Customer provided timestamp of when the asessment took place.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} notes
 *   Optional.
 *
 *   Any notes captured by the recruiter that do not fit into the other
 *   structured fields.
 *
 * @property {Object} rating
 *   Optional.
 *
 *   The rating on this assessment.
 *
 *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
 *
 * @property {number} outcome
 *   Required.
 *
 *   The overall outcome resulting from this exam (passed, failed, nuetral /
 *   inconclusive).
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {Object[]} assessedCompetencies
 *   Optional.
 *
 *   Details of each specific competency / sub-components assessed as part of
 *   this test.
 *
 *   This object should have the same structure as [AssessedCompetency]{@link google.cloud.talent.v4beta1.AssessedCompetency}
 *
 * @typedef Assessment
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Assessment definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Assessment = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Details of a specific assessment competency of one assessment.
   *
   * @property {string} displayName
   *   Required.
   *
   *   A human readable, descriptive name for the given competency.
   *
   * @property {string} notes
   *   Optional.
   *
   *   Any notes captured by the recruiter that do not fit into the other
   *   structured fields.
   *
   * @property {Object} rating
   *   Optional.
   *
   *   The rating on this assessment competency.
   *
   *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
   *
   * @property {number} outcome
   *   Required.
   *
   *   The outcome for this competency (passed, failed, nuetral / inconclusive).
   *
   *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
   *
   * @typedef AssessedCompetency
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.Assessment.AssessedCompetency definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
   */
  AssessedCompetency: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * The type of assessment, such as skill, certification, and so on.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  AssessmentType: {

    /**
     * Default value.
     */
    ASSESSMENT_TYPE_UNSPECIFIED: 0,

    /**
     * Knowledge.
     */
    KNOWLEDGE: 1,

    /**
     * Skill.
     */
    SKILL: 2,

    /**
     * Ability.
     */
    ABILITY: 3,

    /**
     * Psychometric test, such as a personality questionnaire or aptitude test.
     */
    PSYCHOMETRIC: 4,

    /**
     * A certification or license.
     */
    CERTIFICATION: 5,

    /**
     * Screening test, such as a drug test or vision test.
     */
    SCREENING: 6,

    /**
     * A different type of assessment.
     */
    OTHER_ASSESSMENT_TYPE: 7
  }
};

/**
 * Details of an interview.
 *
 * @property {Object} interviewTime
 *   Required.
 *
 *   Customer provided timestamp of when the interview took place.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {string} interviewerId
 *   Optional.
 *
 *   Unique identifier of the person conducting the interview.
 *
 * @property {number} interviewerRole
 *   Required.
 *
 *   Who is conducting the interview.
 *
 *   The number should be among the values of [InterviewerRole]{@link google.cloud.talent.v4beta1.InterviewerRole}
 *
 * @property {number} medium
 *   Optional.
 *
 *   How was the interview conducted (phone, and so on)?
 *
 *   The number should be among the values of [Medium]{@link google.cloud.talent.v4beta1.Medium}
 *
 * @property {string} notes
 *   Optional.
 *
 *   Any notes captured by the interviewer that do not fit into the other
 *   structured fields.
 *
 * @property {Object} rating
 *   Optional.
 *
 *   The rating on this interview.
 *
 *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
 *
 * @property {number} outcome
 *   Required.
 *
 *   The overall decision resulting from this interview (positive, negative,
 *   nuetral).
 *
 *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
 *
 * @property {Object[]} interviewTopicAssessments
 *   Optional.
 *
 *   Interview feedback about individual sub-components (that is, culture fit,
 *   technical skills).
 *
 *   This object should have the same structure as [InterviewTopicAssessment]{@link google.cloud.talent.v4beta1.InterviewTopicAssessment}
 *
 * @typedef Interview
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Interview definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Interview = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * Interview feedback about individual sub-components (for example, culture
   * fit, technical skills, and so on).
   *
   * @property {string} displayName
   *   Required.
   *
   *   What is the sub-topic of asessment detailed here (for example, overall,
   *   culture fit, technical skills, and so on).  May be replaced by an ENUM in
   *   the future.
   *
   * @property {string} notes
   *   Optional.
   *
   *   Any notes captured by the interviewer on this specific area of the
   *   interview.
   *
   * @property {Object} rating
   *   Optional.
   *
   *   The rating on this assessment.
   *
   *   This object should have the same structure as [Rating]{@link google.cloud.talent.v4beta1.Rating}
   *
   * @property {number} outcome
   *   Required.
   *
   *   Is the rating on this asessment area positive, negative, nuetral?
   *
   *   The number should be among the values of [Outcome]{@link google.cloud.talent.v4beta1.Outcome}
   *
   * @typedef InterviewTopicAssessment
   * @memberof google.cloud.talent.v4beta1
   * @see [google.cloud.talent.v4beta1.Interview.InterviewTopicAssessment definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
   */
  InterviewTopicAssessment: {
    // This is for documentation. Actual contents will be loaded by gRPC.
  },

  /**
   * Role of the interviewer.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  InterviewerRole: {

    /**
     * Unknown role of interviewer.
     */
    INTERVIEWER_ROLE_UNSPECIFIED: 0,

    /**
     * Recruiter or sourcer is conducting the interview.
     */
    RECRUITER_OR_SOURCER: 1,

    /**
     * Hiring Manager at the client of the firm that is Google's customer.
     */
    HIRING_MANAGER: 2,

    /**
     * Interview role is known, but this role is not specified in this ENUM.
     */
    OTHER_INTERVIEWER_ROLE: 3
  },

  /**
   * The medium through which an interview is given.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  Medium: {

    /**
     * Unknown interview media.
     */
    MEDIUM_UNSPECIFIED: 0,

    /**
     * Phone interview.
     */
    PHONE: 1,

    /**
     * On-site / on-campus interview.
     */
    ONSITE: 2,

    /**
     * Informal interview (for example, lunch interview).
     */
    INFORMAL: 3,

    /**
     * Video-based interview (for example, video chat).
     */
    VIDEO: 4,

    /**
     * Text-based interview (for example, email, messenger, coding portal).
     */
    TEXT: 5,

    /**
     * Another interview type not specified.
     */
    OTHER_MEDIUM: 6
  }
};

/**
 * Details of an application offer.
 *
 * @property {Object} offerTime
 *   Required.
 *
 *   Timestamp on which this offer was issued to the candidate.
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} expectedStartDate
 *   Required.
 *
 *   Expected start date (inclusive) of the assignment.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} expectedEndDate
 *   Optional.
 *
 *   Expected end date (inclusive) of the assignment. Optional if this offer is
 *   for a full time role.
 *
 *   This object should have the same structure as [Date]{@link google.type.Date}
 *
 * @property {Object} offerExpireTime
 *   Optional.
 *
 *   Timestamp on which this offer expires (that is, by when the candidate
 *   must respond to get the job).
 *
 *   This object should have the same structure as [Timestamp]{@link google.protobuf.Timestamp}
 *
 * @property {Object} offerPayRate
 *   Optional.
 *
 *   Job compensation information for the "pay rate", that is, the
 *   compensation that will paid to the employee
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {Object} offerBillRate
 *   Optional.
 *
 *   Job compensation information for the "bill rate", that is, the amount
 *   that will be billed to the employer.
 *
 *   This object should have the same structure as [CompensationInfo]{@link google.cloud.talent.v4beta1.CompensationInfo}
 *
 * @property {number} offerOutcome
 *   Optional.
 *
 *   Whether or not the offer was accepted by the candidate.
 *
 *   The number should be among the values of [OfferOutcome]{@link google.cloud.talent.v4beta1.OfferOutcome}
 *
 * @property {string} notes
 *   Optional.
 *
 *   Notes from the recruiter about the offer (for example, negotiations,
 *   proposed adjustments).
 *
 * @typedef ApplicationOffer
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.ApplicationOffer definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const ApplicationOffer = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * The overall outcome of an offer.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  OfferOutcome: {

    /**
     * Default value.
     */
    OFFER_OUTCOME_UNSPECIFIED: 0,

    /**
     * Offer declined.
     */
    DECLINED: 1,

    /**
     * Offer accepted.
     */
    ACCEPTED: 2,

    /**
     * Offer pending.
     */
    PENDING: 3
  }
};

/**
 * The details of the score received for an assessment or interview.
 *
 * @property {number} overall
 *   Overall score.
 *
 * @property {number} min
 *   The minimum value for the score.
 *
 * @property {number} max
 *   The maximum value for the score.
 *
 * @property {number} interval
 *   The steps within the score (for example, interval = 1 max = 5
 *   min = 1 indicates that the score can be 1, 2, 3, 4, or 5)
 *
 * @typedef Rating
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.Rating definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const Rating = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * The requirements of a job.
 *
 * @property {number} jobRequirementType
 *   The job requirement type, for example, working overtime, standing, lifting,
 *   and so on.
 *
 *   The number should be among the values of [JobRequirementType]{@link google.cloud.talent.v4beta1.JobRequirementType}
 *
 * @property {string} context
 *   The context for this requirement, for example, if this is for lifting
 *   objects, how much weight?
 *
 * @typedef JobRequirement
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.JobRequirement definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/common.proto}
 */
const JobRequirement = {
  // This is for documentation. Actual contents will be loaded by gRPC.

  /**
   * The enum for job requirement type.
   *
   * @enum {number}
   * @memberof google.cloud.talent.v4beta1
   */
  JobRequirementType: {

    /**
     * Default value.
     */
    JOB_REQUIREMENT_TYPE_UNSPECIFIED: 0,

    /**
     * A job that requires physically demanding work.
     */
    PHYSICAL: 1,

    /**
     * A job that requires or offers overtime work.
     */
    OVERTIME_WORK: 2,

    /**
     * A job that requires or offers weekend work.
     */
    WEEKEND_WORK: 3,

    /**
     * A job that requires travelling for a percentage of the working time.
     */
    PERCENT_TRAVEL: 4,

    /**
     * A job that requires working in a non-climate controlled environment.
     */
    NO_CLIMATE_CONTROL: 5,

    /**
     * A job that requires one or more drug screens.
     */
    DRUG_SCREEN: 6,

    /**
     * A job that requires a smoke-free enviornment.
     */
    SMOKE_FREE: 7,

    /**
     * A job that requires steel-toed shoes.
     */
    STEEL_TOED_SHOES: 8
  }
};

/**
 * Method for commute.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const CommuteMethod = {

  /**
   * Commute method isn't specified.
   */
  COMMUTE_METHOD_UNSPECIFIED: 0,

  /**
   * Commute time is calculated based on driving time.
   */
  DRIVING: 1,

  /**
   * Commute time is calculated based on public transit including bus, metro,
   * subway, and so on.
   */
  TRANSIT: 2,

  /**
   * Commute time is calculated based on walking time.
   */
  WALKING: 3,

  /**
   * Commute time is calculated based on biking time.
   */
  CYCLING: 4
};

/**
 * An enum that represents the size of the company.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const CompanySize = {

  /**
   * Default value if the size isn't specified.
   */
  COMPANY_SIZE_UNSPECIFIED: 0,

  /**
   * The company has less than 50 employees.
   */
  MINI: 1,

  /**
   * The company has between 50 and 99 employees.
   */
  SMALL: 2,

  /**
   * The company has between 100 and 499 employees.
   */
  SMEDIUM: 3,

  /**
   * The company has between 500 and 999 employees.
   */
  MEDIUM: 4,

  /**
   * The company has between 1,000 and 4,999 employees.
   */
  BIG: 5,

  /**
   * The company has between 5,000 and 9,999 employees.
   */
  BIGGER: 6,

  /**
   * The company has 10,000 or more employees.
   */
  GIANT: 7
};

/**
 * Enum that represents the usage of the contact information.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const ContactInfoUsage = {

  /**
   * Default value.
   */
  CONTACT_INFO_USAGE_UNSPECIFIED: 0,

  /**
   * Personal use.
   */
  PERSONAL: 1,

  /**
   * Work use.
   */
  WORK: 2,

  /**
   * School use.
   */
  SCHOOL: 3
};

/**
 * Educational degree level defined in International Standard Classification
 * of Education (ISCED).
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const DegreeType = {

  /**
   * Default value. Represents no degree, or early childhood education.
   * Maps to ISCED code 0.
   * Ex) Kindergarten
   */
  DEGREE_TYPE_UNSPECIFIED: 0,

  /**
   * Primary education which is typically the first stage of compulsory
   * education. ISCED code 1.
   * Ex) Elementary school
   */
  PRIMARY_EDUCATION: 1,

  /**
   * Lower secondary education; First stage of secondary education building on
   * primary education, typically with a more subject-oriented curriculum.
   * ISCED code 2.
   * Ex) Middle school
   */
  LOWER_SECONDARY_EDUCATION: 2,

  /**
   * Middle education; Second/final stage of secondary education preparing for
   * tertiary education and/or providing skills relevant to employment.
   * Usually with an increased range of subject options and streams. ISCED
   * code 3.
   * Ex) High school
   */
  UPPER_SECONDARY_EDUCATION: 3,

  /**
   * Adult Remedial Education; Programmes providing learning experiences that
   * build on secondary education and prepare for labour market entry and/or
   * tertiary education. The content is broader than secondary but not as
   * complex as tertiary education. ISCED code 4.
   */
  ADULT_REMEDIAL_EDUCATION: 4,

  /**
   * Associate's or equivalent; Short first tertiary programmes that are
   * typically practically-based, occupationally-specific and prepare for
   * labour market entry. These programmes may also provide a pathway to other
   * tertiary programmes. ISCED code 5.
   */
  ASSOCIATES_OR_EQUIVALENT: 5,

  /**
   * Bachelor's or equivalent; Programmes designed to provide intermediate
   * academic and/or professional knowledge, skills and competencies leading
   * to a first tertiary degree or equivalent qualification. ISCED code 6.
   */
  BACHELORS_OR_EQUIVALENT: 6,

  /**
   * Master's or equivalent; Programmes designed to provide advanced academic
   * and/or professional knowledge, skills and competencies leading to a
   * second tertiary degree or equivalent qualification. ISCED code 7.
   */
  MASTERS_OR_EQUIVALENT: 7,

  /**
   * Doctoral or equivalent; Programmes designed primarily to lead to an
   * advanced research qualification, usually concluding with the submission
   * and defense of a substantive dissertation of publishable quality based on
   * original research. ISCED code 8.
   */
  DOCTORAL_OR_EQUIVALENT: 8
};

/**
 * An enum that represents the employment type of a job.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const EmploymentType = {

  /**
   * The default value if the employment type isn't specified.
   */
  EMPLOYMENT_TYPE_UNSPECIFIED: 0,

  /**
   * The job requires working a number of hours that constitute full
   * time employment, typically 40 or more hours per week.
   */
  FULL_TIME: 1,

  /**
   * The job entails working fewer hours than a full time job,
   * typically less than 40 hours a week.
   */
  PART_TIME: 2,

  /**
   * The job is offered as a contracted, as opposed to a salaried employee,
   * position.
   */
  CONTRACTOR: 3,

  /**
   * The job is offered as a contracted position with the understanding
   * that it's converted into a full-time position at the end of the
   * contract. Jobs of this type are also returned by a search for
   * EmploymentType.CONTRACTOR jobs.
   */
  CONTRACT_TO_HIRE: 4,

  /**
   * The job is offered as a temporary employment opportunity, usually
   * a short-term engagement.
   */
  TEMPORARY: 5,

  /**
   * The job is a fixed-term opportunity for students or entry-level job
   * seekers to obtain on-the-job training, typically offered as a summer
   * position.
   */
  INTERN: 6,

  /**
   * The is an opportunity for an individual to volunteer, where there's no
   * expectation of compensation for the provided services.
   */
  VOLUNTEER: 7,

  /**
   * The job requires an employee to work on an as-needed basis with a
   * flexible schedule.
   */
  PER_DIEM: 8,

  /**
   * The job involves employing people in remote areas and flying them
   * temporarily to the work site instead of relocating employees and their
   * families permanently.
   */
  FLY_IN_FLY_OUT: 9,

  /**
   * This job is a permanent placement from a staffing agency at an
   * organization.
   */
  PERMANENT: 11,

  /**
   * This job is for a specific deliverable ("gig"), for example, delivering a
   * specific project.
   */
  DELIVERABLE: 12,

  /**
   * The job does not fit any of the other listed types.
   */
  OTHER_EMPLOYMENT_TYPE: 10
};

/**
 * Input only.
 *
 * Option for HTML content sanitization on user input fields, for example, job
 * description. By setting this option, user can determine whether and how
 * sanitization is performed on these fields.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const HtmlSanitization = {

  /**
   * Default value.
   */
  HTML_SANITIZATION_UNSPECIFIED: 0,

  /**
   * Disables sanitization on HTML input.
   */
  HTML_SANITIZATION_DISABLED: 1,

  /**
   * Sanitizes HTML input, only accepts bold, italic, ordered list, and
   * unordered list markup tags.
   */
  SIMPLE_FORMATTING_ONLY: 2
};

/**
 * An enum that represents employee benefits included with the job.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const JobBenefit = {

  /**
   * Default value if the type isn't specified.
   */
  JOB_BENEFIT_UNSPECIFIED: 0,

  /**
   * The job includes access to programs that support child care, such
   * as daycare.
   */
  CHILD_CARE: 1,

  /**
   * The job includes dental services covered by a dental
   * insurance plan.
   */
  DENTAL: 2,

  /**
   * The job offers specific benefits to domestic partners.
   */
  DOMESTIC_PARTNER: 3,

  /**
   * The job allows for a flexible work schedule.
   */
  FLEXIBLE_HOURS: 4,

  /**
   * The job includes health services covered by a medical insurance plan.
   */
  MEDICAL: 5,

  /**
   * The job includes a life insurance plan provided by the employer or
   * available for purchase by the employee.
   */
  LIFE_INSURANCE: 6,

  /**
   * The job allows for a leave of absence to a parent to care for a newborn
   * child.
   */
  PARENTAL_LEAVE: 7,

  /**
   * The job includes a workplace retirement plan provided by the
   * employer or available for purchase by the employee.
   */
  RETIREMENT_PLAN: 8,

  /**
   * The job allows for paid time off due to illness.
   */
  SICK_DAYS: 9,

  /**
   * The job includes paid time off for vacation.
   */
  VACATION: 10,

  /**
   * The job includes vision services covered by a vision
   * insurance plan.
   */
  VISION: 11
};

/**
 * An enum that represents the categorization or primary focus of specific
 * role. This value is different than the "industry" associated with a role,
 * which is related to the categorization of the company listing the job.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const JobCategory = {

  /**
   * The default value if the category isn't specified.
   */
  JOB_CATEGORY_UNSPECIFIED: 0,

  /**
   * An accounting and finance job, such as an Accountant.
   */
  ACCOUNTING_AND_FINANCE: 1,

  /**
   * An administrative and office job, such as an Administrative Assistant.
   */
  ADMINISTRATIVE_AND_OFFICE: 2,

  /**
   * An advertising and marketing job, such as Marketing Manager.
   */
  ADVERTISING_AND_MARKETING: 3,

  /**
   * An animal care job, such as Veterinarian.
   */
  ANIMAL_CARE: 4,

  /**
   * An art, fashion, or design job, such as Designer.
   */
  ART_FASHION_AND_DESIGN: 5,

  /**
   * A business operations job, such as Business Operations Manager.
   */
  BUSINESS_OPERATIONS: 6,

  /**
   * A cleaning and facilities job, such as Custodial Staff.
   */
  CLEANING_AND_FACILITIES: 7,

  /**
   * A computer and IT job, such as Systems Administrator.
   */
  COMPUTER_AND_IT: 8,

  /**
   * A construction job, such as General Laborer.
   */
  CONSTRUCTION: 9,

  /**
   * A customer service job, such s Cashier.
   */
  CUSTOMER_SERVICE: 10,

  /**
   * An education job, such as School Teacher.
   */
  EDUCATION: 11,

  /**
   * An entertainment and travel job, such as Flight Attendant.
   */
  ENTERTAINMENT_AND_TRAVEL: 12,

  /**
   * A farming or outdoor job, such as Park Ranger.
   */
  FARMING_AND_OUTDOORS: 13,

  /**
   * A healthcare job, such as Registered Nurse.
   */
  HEALTHCARE: 14,

  /**
   * A human resources job, such as Human Resources Director.
   */
  HUMAN_RESOURCES: 15,

  /**
   * An installation, maintenance, or repair job, such as Electrician.
   */
  INSTALLATION_MAINTENANCE_AND_REPAIR: 16,

  /**
   * A legal job, such as Law Clerk.
   */
  LEGAL: 17,

  /**
   * A management job, often used in conjunction with another category,
   * such as Store Manager.
   */
  MANAGEMENT: 18,

  /**
   * A manufacturing or warehouse job, such as Assembly Technician.
   */
  MANUFACTURING_AND_WAREHOUSE: 19,

  /**
   * A media, communications, or writing job, such as Media Relations.
   */
  MEDIA_COMMUNICATIONS_AND_WRITING: 20,

  /**
   * An oil, gas or mining job, such as Offshore Driller.
   */
  OIL_GAS_AND_MINING: 21,

  /**
   * A personal care and services job, such as Hair Stylist.
   */
  PERSONAL_CARE_AND_SERVICES: 22,

  /**
   * A protective services job, such as Security Guard.
   */
  PROTECTIVE_SERVICES: 23,

  /**
   * A real estate job, such as Buyer's Agent.
   */
  REAL_ESTATE: 24,

  /**
   * A restaurant and hospitality job, such as Restaurant Server.
   */
  RESTAURANT_AND_HOSPITALITY: 25,

  /**
   * A sales and/or retail job, such Sales Associate.
   */
  SALES_AND_RETAIL: 26,

  /**
   * A science and engineering job, such as Lab Technician.
   */
  SCIENCE_AND_ENGINEERING: 27,

  /**
   * A social services or non-profit job, such as Case Worker.
   */
  SOCIAL_SERVICES_AND_NON_PROFIT: 28,

  /**
   * A sports, fitness, or recreation job, such as Personal Trainer.
   */
  SPORTS_FITNESS_AND_RECREATION: 29,

  /**
   * A transportation or logistics job, such as Truck Driver.
   */
  TRANSPORTATION_AND_LOGISTICS: 30
};

/**
 * An enum that represents the required experience level required for the job.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const JobLevel = {

  /**
   * The default value if the level isn't specified.
   */
  JOB_LEVEL_UNSPECIFIED: 0,

  /**
   * Entry-level individual contributors, typically with less than 2 years of
   * experience in a similar role. Includes interns.
   */
  ENTRY_LEVEL: 1,

  /**
   * Experienced individual contributors, typically with 2+ years of
   * experience in a similar role.
   */
  EXPERIENCED: 2,

  /**
   * Entry- to mid-level managers responsible for managing a team of people.
   */
  MANAGER: 3,

  /**
   * Senior-level managers responsible for managing teams of managers.
   */
  DIRECTOR: 4,

  /**
   * Executive-level managers and above, including C-level positions.
   */
  EXECUTIVE: 5
};

/**
 * The overall outcome /decision / result indicator.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const Outcome = {

  /**
   * Default value.
   */
  OUTCOME_UNSPECIFIED: 0,

  /**
   * A positive outcome / passing indicator (for example, candidate was
   * recommended for hiring or to be moved forward in the hiring process,
   * candidate passed a test).
   */
  POSITIVE: 1,

  /**
   * A neutral outcome / no clear indicator (for example, no strong
   * reccommendation either to move forward / not move forward, neutral score).
   */
  NEUTRAL: 2,

  /**
   * A negative outcome / failing indicator (for example, candidate was
   * recommended to NOT move forward in the hiring process, failed a test).
   */
  NEGATIVE: 3,

  /**
   * The assessment outcome is not available or otherwise unknown (for example,
   * candidate did not complete assessment).
   */
  OUTCOME_NOT_AVAILABLE: 4
};

/**
 * An enum that represents the job posting region. In most cases, job postings
 * don't need to specify a region. If a region is given, jobs are
 * eligible for searches in the specified region.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const PostingRegion = {

  /**
   * If the region is unspecified, the job is only returned if it
   * matches the LocationFilter.
   */
  POSTING_REGION_UNSPECIFIED: 0,

  /**
   * In addition to exact location matching, job posting is returned when the
   * LocationFilter in the search query is in the same administrative area
   * as the returned job posting. For example, if a `ADMINISTRATIVE_AREA` job
   * is posted in "CA, USA", it's returned if LocationFilter has
   * "Mountain View".
   *
   * Administrative area refers to top-level administrative subdivision of this
   * country. For example, US state, IT region, UK constituent nation and
   * JP prefecture.
   */
  ADMINISTRATIVE_AREA: 1,

  /**
   * In addition to exact location matching, job is returned when
   * LocationFilter in search query is in the same country as this job.
   * For example, if a `NATION_WIDE` job is posted in "USA", it's
   * returned if LocationFilter has 'Mountain View'.
   */
  NATION: 2,

  /**
   * Job allows employees to work remotely (telecommute).
   * If locations are provided with this value, the job is
   * considered as having a location, but telecommuting is allowed.
   */
  TELECOMMUTE: 3
};

/**
 * The role of the person providing the performance review.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const ReviewerRole = {

  /**
   * Default value.
   */
  REVIEWER_ROLE_UNSPECIFIED: 0,

  /**
   * Performance review is done by the employee's manager.
   */
  EMPLOYEE_MANAGER: 1,

  /**
   * Performance review is done by a peer of the employee.
   */
  PEER: 2,

  /**
   * Performance review is done by a subordinate of the employee.
   */
  SUBORDINATE: 3,

  /**
   * Reviewer role is known, but this role is not specified in this ENUM.
   */
  OTHER_REVIEWER_ROLE: 4
};

/**
 * Enum that represents the skill proficiency level.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const SkillProficiencyLevel = {

  /**
   * Default value.
   */
  SKILL_PROFICIENCY_LEVEL_UNSPECIFIED: 0,

  /**
   * Lacks any proficiency in this skill.
   */
  UNSKILLED: 6,

  /**
   * Have a common knowledge or an understanding of basic techniques and
   * concepts.
   */
  FUNDAMENTAL_AWARENESS: 1,

  /**
   * Have the level of experience gained in a classroom and/or experimental
   * scenarios or as a trainee on-the-job.
   */
  NOVICE: 2,

  /**
   * Be able to successfully complete tasks in this skill as requested. Help
   * from an expert may be required from time to time, but can usually perform
   * skill independently.
   */
  INTERMEDIATE: 3,

  /**
   * Can perform the actions associated with this skill without assistance.
   */
  ADVANCED: 4,

  /**
   * Known as an expert in this area.
   */
  EXPERT: 5
};

/**
 * An enum that represents who has view access to the resource.
 *
 * @enum {number}
 * @memberof google.cloud.talent.v4beta1
 */
const Visibility = {

  /**
   * Default value.
   */
  VISIBILITY_UNSPECIFIED: 0,

  /**
   * The resource is only visible to the GCP account who owns it.
   */
  ACCOUNT_ONLY: 1,

  /**
   * The resource is visible to the owner and may be visible to other
   * applications and processes at Google.
   */
  SHARED_WITH_GOOGLE: 2,

  /**
   * The resource is visible to the owner and may be visible to all other API
   * clients.
   */
  SHARED_WITH_PUBLIC: 3
};