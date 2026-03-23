import { create } from 'zustand'
import { devtools } from 'zustand/middleware'
import type { StudentBadgeBasicDto } from '@/api/generated/model'
import type { QuizPhase } from '@/hooks/useQuizEngine'

interface QuizState {
  activeQuizId: string | null
  phase: QuizPhase
  attemptId: number | null
  currentQuestionId: number | null
  nextQuestionId: number | null
  questionStartTime: number
  lastResponseTimeMs: number | null
  correctFreeTextAnswer: string | null
  correctAnswersCount: number
  questionsAnswered: number
  selectedOptionIds: (number | string)[]
  freeTextValue: string
  earnedBadges: StudentBadgeBasicDto[]
  currentCorrectOptionIds: (number | string)[]

  actions: {
    initResume: (attemptId: number) => void
    startAttempt: (attemptId: number, firstQuestionId: number | null) => void
    finishTransition: () => void
    selectOption: (optId: number | string) => void
    setFreeText: (text: string) => void
    submitAnswer: (payload: {
      correctIds: (number | string)[]
      nextQuestionId: number | null
      correctFreeTextAnswer: string | null
      responseTimeMs: number
      isFullyCorrect: boolean
    }) => void
    continueToNext: () => void
    completeQuiz: (badges?: StudentBadgeBasicDto[]) => void
    resetQuiz: () => void
  }
}

const initialState = {
  activeQuizId: null,
  phase: 'start' as QuizPhase,
  attemptId: null,
  currentQuestionId: null,
  nextQuestionId: null,
  questionStartTime: Date.now(),
  lastResponseTimeMs: null,
  correctFreeTextAnswer: null,
  correctAnswersCount: 0,
  questionsAnswered: 0,
  selectedOptionIds: [],
  freeTextValue: '',
  earnedBadges: [],
  currentCorrectOptionIds: [],
}

export const useQuizStore = create<QuizState>()(
  devtools((set) => ({
    ...initialState,

    actions: {
      initResume: (attemptId) =>
        set({
          ...initialState,
          attemptId,
          phase: 'resuming',
        }),

      startAttempt: (attemptId, firstQuestionId) =>
        set({
          attemptId,
          currentQuestionId: firstQuestionId,
          phase: firstQuestionId ? 'transition' : 'completed',
        }),

      finishTransition: () =>
        set({
          phase: 'answering',
          questionStartTime: Date.now(),
          selectedOptionIds: [],
          freeTextValue: '',
          currentCorrectOptionIds: [],
        }),

      selectOption: (optId) => set({ selectedOptionIds: [optId] }),

      setFreeText: (text) => set({ freeTextValue: text }),

      submitAnswer: (payload) =>
        set((state) => ({
          phase: 'recap',
          currentCorrectOptionIds: payload.correctIds,
          nextQuestionId: payload.nextQuestionId,
          correctFreeTextAnswer: payload.correctFreeTextAnswer,
          lastResponseTimeMs: payload.responseTimeMs,
          correctAnswersCount:
            state.correctAnswersCount + (payload.isFullyCorrect ? 1 : 0),
          questionsAnswered: state.questionsAnswered + 1,
        })),

      continueToNext: () =>
        set((state) => ({
          currentQuestionId: state.nextQuestionId,
          phase: 'transition',

          selectedOptionIds: [],
          freeTextValue: '',
          currentCorrectOptionIds: [],
          correctFreeTextAnswer: null,
        })),

      completeQuiz: (badges) =>
        set((state) => ({
          phase: 'completed',
          earnedBadges: badges || state.earnedBadges,
        })),

      resetQuiz: () => set(initialState),
    },
  })),
)
