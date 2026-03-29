import { create } from 'zustand'
import { type UserDTO } from '@/api/generated/model'

interface AuthState {
  user: UserDTO | null
  isAuthenticated: boolean
  isAdmin: boolean
  isTeacher: boolean
  setUser: (user: UserDTO | null) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  isAdmin: false,
  isTeacher: false,
  setUser: (user) =>
    set({
      user,
      isAuthenticated: !!user,
      isAdmin: user?.role === 'Admin',
      isTeacher: user?.role === 'Teacher' || user?.role === 'Admin',
    }),
  logout: () =>
    set({
      user: null,
      isAuthenticated: false,
      isAdmin: false,
      isTeacher: false,
    }),
}))
