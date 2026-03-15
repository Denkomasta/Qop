import { getApiAdminsId } from '@/api/generated/endpoints/admins/admins'
import { getApiStudentsId } from '@/api/generated/endpoints/students/students'
import { getApiTeachersId } from '@/api/generated/endpoints/teachers/teachers'
import type {
  AdminDto,
  StudentDto,
  TeacherDto,
  UserRole,
} from '@/api/generated/model'
import { useQuery } from '@tanstack/react-query'

export type ExtendedProfileDto = AdminDto | StudentDto | TeacherDto

export const useExtendedUserProfile = (
  id?: number | string,
  role?: UserRole,
) => {
  return useQuery({
    queryKey: ['extended-profile', role, id],
    queryFn: async ({ signal }): Promise<ExtendedProfileDto> => {
      if (!id || !role) throw new Error('Missing user ID or role')

      switch (role.toLowerCase()) {
        case 'admin':
          return getApiAdminsId(id, undefined, signal)
        case 'student':
          return getApiStudentsId(id, undefined, signal)
        case 'teacher':
          return getApiTeachersId(id, undefined, signal)
        default:
          throw new Error(`Unsupported role: ${role}`)
      }
    },
    enabled: !!id && !!role,
    staleTime: 1000 * 60 * 5,
  })
}
