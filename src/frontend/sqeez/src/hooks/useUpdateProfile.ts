import { useMutation, useQueryClient } from '@tanstack/react-query'
import type {
  PatchAdminDto,
  PatchStudentDto,
  PatchTeacherDto,
  UserRole,
} from '@/api/generated/model'
import { patchApiStudentsId } from '@/api/generated/endpoints/students/students'
import { patchApiTeachersId } from '@/api/generated/endpoints/teachers/teachers'
import { patchApiAdminsId } from '@/api/generated/endpoints/admins/admins'
import { getExtendedProfileQueryKey } from './useExtendedUserProfile'
import { getGetApiAuthMeQueryKey } from '@/api/generated/endpoints/auth/auth'

export type ProfilePatchPayload =
  | PatchStudentDto
  | PatchTeacherDto
  | PatchAdminDto

export const useUpdateProfile = (id?: number | string, role?: UserRole) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (patchData: ProfilePatchPayload) => {
      if (!id || !role) throw new Error('Missing user ID or role')

      switch (role.toLowerCase()) {
        case 'student':
          return patchApiStudentsId(id, patchData as PatchStudentDto)
        case 'teacher':
          return patchApiTeachersId(id, patchData as PatchTeacherDto)
        case 'admin':
          return patchApiAdminsId(id, patchData as PatchAdminDto)
        default:
          throw new Error(`Unsupported role: ${role}`)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: getExtendedProfileQueryKey(role, id),
      })

      queryClient.invalidateQueries({
        queryKey: getGetApiAuthMeQueryKey(),
      })
    },
  })
}
