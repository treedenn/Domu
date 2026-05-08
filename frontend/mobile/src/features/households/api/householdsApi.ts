import { apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';

export enum HouseholdSubscriptionPlan {
  Unknown = 0,
  Free = 1,
  Premium = 2,
}

export enum HouseholdSubscriptionStatus {
  Unknown = 0,
  Active = 1,
  CancellationScheduled = 2,
  Expired = 3,
}

export enum HouseholdMemberRole {
  Unspecified = 0,
  Owner = 1,
  Admin = 2,
  Member = 3,
}

export enum HouseholdInvitationStatus {
  Unknown = 0,
  Pending = 1,
  Accepted = 2,
  Cancelled = 3,
  Expired = 4,
}

export type HouseholdView = {
  id: string;
  ownerId: string;
  name: string;
  subscriptionPlan: HouseholdSubscriptionPlan;
  subscriptionStatus: HouseholdSubscriptionStatus;
  subscriptionCurrentPeriodEndsAt: string | null;
  subscriptionCancelledAt: string | null;
};

export type HouseholdMemberView = {
  id: string;
  householdId: string;
  userId: string;
  role: HouseholdMemberRole;
  joinedAt: string;
};

export type HouseholdInvitationView = {
  id: string;
  householdId: string;
  email: string;
  invitedByUserId: string;
  role: HouseholdMemberRole;
  status: HouseholdInvitationStatus;
  createdAt: string;
  expiresAt: string;
  acceptedAt: string | null;
};

export type CreateHouseholdRequest = {
  name: string;
};

export type UpdateHouseholdRequest = CreateHouseholdRequest;

export type InviteHouseholdMemberRequest = {
  email: string;
  role?: HouseholdMemberRole;
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

