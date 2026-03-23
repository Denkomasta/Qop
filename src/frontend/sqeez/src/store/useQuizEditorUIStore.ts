import { create } from 'zustand'

interface QuizEditorUIState {
  activeQuestionId: number | null
  isSidebarOpen: boolean

  actions: {
    selectQuestion: (questionId: number | null) => void
    toggleSidebar: () => void
    resetEditor: () => void
  }
}

export const useQuizEditorUIStore = create<QuizEditorUIState>((set) => ({
  activeQuestionId: null,
  isSidebarOpen: true,

  actions: {
    selectQuestion: (questionId) => set({ activeQuestionId: questionId }),
    toggleSidebar: () =>
      set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),
    resetEditor: () => set({ activeQuestionId: null, isSidebarOpen: true }),
  },
}))
