import { create } from 'zustand'
import { type UserDTO } from '@/api/generated/model'

interface AuthState {
  user: UserDTO | null
  isAuthenticated: boolean
  setUser: (user: UserDTO | null) => void
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  setUser: (user) =>
    set({
      user,
      isAuthenticated: !!user,
    }),
  logout: () =>
    set({
      user: null,
      isAuthenticated: false,
    }),
}))
