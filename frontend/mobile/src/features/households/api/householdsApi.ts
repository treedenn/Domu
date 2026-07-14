import { apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';

export enum HouseholdSubscriptionPlan {
  Unknown = 'unknown',
  Free = 'free',
  Premium = 'premium',
}

export enum HouseholdSubscriptionStatus {
  Unknown = 'unknown',
  Active = 'active',
  CancellationScheduled = 'cancellationScheduled',
  Expired = 'expired',
}

export enum HouseholdMemberRole {
  Unspecified = 'unspecified',
  Owner = 'owner',
  Admin = 'admin',
  Member = 'member',
}

export enum HouseholdInvitationStatus {
  Unknown = 'unknown',
  Pending = 'pending',
  Accepted = 'accepted',
  Cancelled = 'cancelled',
  Expired = 'expired',
}

export type HouseholdView = {
  id: string;
  name: string;
  subscriptionPlan: HouseholdSubscriptionPlan;
  subscriptionStatus: HouseholdSubscriptionStatus;
  subscriptionCurrentPeriodEndsAt: string | null;
  subscriptionCancelledAt: string | null;
};

export type HouseholdMemberView = {
  id: string;
  householdId: string;
  userId: string | null;
  displayName: string;
  role: HouseholdMemberRole;
  joinedAt: string;
  archived: boolean;
};

export type HouseholdInvitationView = {
  id: string;
  householdId: string;
  email: string;
  displayName: string;
  invitedByMemberId: string;
  role: HouseholdMemberRole;
  status: HouseholdInvitationStatus;
  createdAt: string;
  expiresAt: string;
  acceptedAt: string | null;
};

export type CreateHouseholdRequest = {
  name: string;
  ownerDisplayName: string;
};

export type UpdateHouseholdRequest = {
  name: string;
};

export type InviteHouseholdMemberRequest = {
  email: string;
  displayName: string;
  role?: HouseholdMemberRole;
};

export type UpdateHouseholdMemberRequest = {
  displayName: string;
  role: HouseholdMemberRole;
  archived: boolean;
};

export function getHouseholds(options?: ApiRequestOptions) {
  return apiRequest<HouseholdView[]>('/households', options);
}

export function getHousehold(householdId: string, options?: ApiRequestOptions) {
  return apiRequest<HouseholdView>(`/households/${householdId}`, options);
}

export function createHousehold(request: CreateHouseholdRequest, options?: ApiRequestOptions) {
  return apiRequest<HouseholdView>('/households', {
    ...options,
    body: request,
    method: 'POST',
  });
}

export function updateHousehold(
  householdId: string,
  request: UpdateHouseholdRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<HouseholdView>(`/households/${householdId}`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function deleteHousehold(householdId: string, options?: ApiRequestOptions) {
  return apiRequest<void>(`/households/${householdId}`, {
    ...options,
    method: 'DELETE',
  });
}

export function getHouseholdMembers(householdId: string, options?: ApiRequestOptions) {
  return apiRequest<HouseholdMemberView[]>(`/households/${householdId}/members`, options);
}

export function getHouseholdMember(
  householdId: string,
  memberId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<HouseholdMemberView>(
    `/households/${householdId}/members/${memberId}`,
    options,
  );
}

export function updateHouseholdMember(
  householdId: string,
  memberId: string,
  request: UpdateHouseholdMemberRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<HouseholdMemberView>(`/households/${householdId}/members/${memberId}`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function getHouseholdInvitations(householdId: string, options?: ApiRequestOptions) {
  return apiRequest<HouseholdInvitationView[]>(`/households/${householdId}/invitations`, options);
}

export function inviteHouseholdMember(
  householdId: string,
  request: InviteHouseholdMemberRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<HouseholdInvitationView>(`/households/${householdId}/invitations`, {
    ...options,
    body: request,
    method: 'POST',
  });
}

export function acceptHouseholdInvitation(token: string, options?: ApiRequestOptions) {
  return apiRequest<HouseholdMemberView>(`/households/invitations/${token}/accept`, {
    ...options,
    method: 'POST',
  });
}
