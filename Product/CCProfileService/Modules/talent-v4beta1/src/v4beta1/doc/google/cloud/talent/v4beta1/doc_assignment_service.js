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
 * The Request of the CreateAssignment method.
 *
 * @property {string} parent
 *   Required.
 *
 *   Resource name of the project under which the assignment is created.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}", for
 *   example, "projects/test-project/tenants/test-tenant/profiles/test-profile".
 *
 * @property {Object} assignment
 *   Required.
 *
 *   The assignment to be created.
 *
 *   This object should have the same structure as [Assignment]{@link google.cloud.talent.v4beta1.Assignment}
 *
 * @typedef CreateAssignmentRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.CreateAssignmentRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment_service.proto}
 */
const CreateAssignmentRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Request for getting a assignment by name.
 *
 * @property {string} name
 *   Required.
 *
 *   The resource name of the assignment to be retrieved.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/assignments/{assignment_id}",
 *   for example,
 *   "projects/test-project/tenants/test-tenant/profiles/test-profile/assignments/test-assignment".
 *
 * @typedef GetAssignmentRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.GetAssignmentRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment_service.proto}
 */
const GetAssignmentRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Request for updating a specified assignment.
 *
 * @property {Object} assignment
 *   Required.
 *
 *   The assignment resource to replace the current resource in the system.
 *
 *   This object should have the same structure as [Assignment]{@link google.cloud.talent.v4beta1.Assignment}
 *
 * @property {Object} updateMask
 *   Optional but strongly recommended for the best service
 *   experience.
 *
 *   If update_mask is provided, only the specified fields in
 *   assignment are updated. Otherwise all the fields are updated.
 *
 *   A field mask to specify the assignment fields to be updated. Only
 *   top level fields of Assignment are supported.
 *
 *   This object should have the same structure as [FieldMask]{@link google.protobuf.FieldMask}
 *
 * @typedef UpdateAssignmentRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.UpdateAssignmentRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment_service.proto}
 */
const UpdateAssignmentRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Request to delete a assignment.
 *
 * @property {string} name
 *   Required.
 *
 *   The resource name of the assignment to be deleted.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}/assignments/{assignment_id}",
 *   for example,
 *   "projects/test-project/tenants/test-tenant/profiles/test-profile/assignments/test-assignment".
 *
 * @typedef DeleteAssignmentRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.DeleteAssignmentRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment_service.proto}
 */
const DeleteAssignmentRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * List assignments for which the client has ACL visibility.
 *
 * @property {string} parent
 *   Required.
 *
 *   Resource name of the project under which the assignment is created.
 *
 *   The format is
 *   "projects/{project_id}/tenants/{tenant_id}/profiles/{profile_id}", for
 *   example, "projects/test-project/tenants/test-tenant/profiles/test-profile".
 *
 * @property {string} pageToken
 *   Optional.
 *
 *   The starting indicator from which to return results.
 *
 * @property {number} pageSize
 *   Optional.
 *
 *   The maximum number of assignments to be returned, at most 100.
 *   Default is 100 if a non-positive number is provided.
 *
 * @typedef ListAssignmentsRequest
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.ListAssignmentsRequest definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment_service.proto}
 */
const ListAssignmentsRequest = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};

/**
 * Output only.
 *
 * The List assignments response object.
 *
 * @property {Object[]} assignments
 *   Assignments for the current client.
 *
 *   This object should have the same structure as [Assignment]{@link google.cloud.talent.v4beta1.Assignment}
 *
 * @property {string} nextPageToken
 *   A token to retrieve the next page of results.
 *
 * @property {Object} metadata
 *   Additional information for the API invocation, such as the request
 *   tracking id.
 *
 *   This object should have the same structure as [ResponseMetadata]{@link google.cloud.talent.v4beta1.ResponseMetadata}
 *
 * @typedef ListAssignmentsResponse
 * @memberof google.cloud.talent.v4beta1
 * @see [google.cloud.talent.v4beta1.ListAssignmentsResponse definition in proto format]{@link https://github.com/googleapis/googleapis/blob/master/google/cloud/talent/v4beta1/assignment_service.proto}
 */
const ListAssignmentsResponse = {
  // This is for documentation. Actual contents will be loaded by gRPC.
};