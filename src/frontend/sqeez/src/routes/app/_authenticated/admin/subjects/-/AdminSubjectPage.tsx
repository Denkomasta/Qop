import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { BookCopy, Search, Plus } from 'lucide-react'

import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import { Button } from '@/components/ui/Button'
import { useGetApiSubjects } from '@/api/generated/endpoints/subjects/subjects'
import type { SubjectDto } from '@/api/generated/model'

import { AdminSubjectsTable } from './AdminSubjectsTable'
import { CreateSubjectModal } from './CreateSubjectModal'
import { EditSubjectModal } from './EditSubjectModal'
import { DeleteSubjectModal } from './DeleteSubjectModal'

export function AdminSubjectsPage() {
  const { t } = useTranslation()

  const [searchQuery, setSearchQuery] = useState('')
  const [codeFilter, setCodeFilter] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 15

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [subjectToEdit, setSubjectToEdit] = useState<SubjectDto | null>(null)
  const [subjectToDelete, setSubjectToDelete] = useState<SubjectDto | null>(
    null,
  )

  const { data: subjectsResponse, isLoading } = useGetApiSubjects({
    SearchTerm: searchQuery || undefined,
    Code: codeFilter || undefined,
    PageNumber: pageNumber,
    PageSize: pageSize,
  })

  const subjects = subjectsResponse?.data || []
  const totalPages = Number(subjectsResponse?.totalPages || 1)
  const totalCount = subjectsResponse?.totalCount || 0

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="border-b border-border bg-card p-6">
        <div className="mx-auto flex max-w-7xl flex-col gap-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-cyan-500/10 text-cyan-500">
                <BookCopy className="h-6 w-6" />
              </div>
              <div>
                <h1 className="text-3xl font-bold tracking-tight">
                  {t('admin.subjects.subjectManagement')}
                </h1>
                <p className="text-muted-foreground">
                  {t('admin.subjects.totalSubjects')}:{' '}
                  <span className="font-bold">{totalCount}</span>
                </p>
              </div>
            </div>

            <Button
              onClick={() => setIsCreateModalOpen(true)}
              className="gap-2"
            >
              <Plus className="h-4 w-4" />
              {t('admin.subjects.createSubject')}
            </Button>
          </div>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <DebouncedInput
              id="admin-subject-search"
              value={searchQuery}
              onChange={(val) => {
                setSearchQuery(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.subjects.searchPlaceholder')}
              icon={<Search className="h-4 w-4" />}
              className="max-w-md bg-background"
              hideErrors
            />

            <DebouncedInput
              id="admin-subject-code"
              value={codeFilter}
              onChange={(val) => {
                setCodeFilter(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.subjects.filterCode')}
              className="max-w-62.5 bg-background"
              hideErrors
            />
          </div>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        <div className="mx-auto max-w-7xl">
          <AdminSubjectsTable
            subjects={subjects}
            isLoading={isLoading}
            onEditSubject={setSubjectToEdit}
            onDeleteSubject={setSubjectToDelete}
          />

          {!isLoading && totalPages > 1 && (
            <div className="mt-6 flex justify-center">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            </div>
          )}
        </div>
      </div>

      <CreateSubjectModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
      />
      <EditSubjectModal
        isOpen={!!subjectToEdit}
        subject={subjectToEdit}
        onClose={() => setSubjectToEdit(null)}
      />
      <DeleteSubjectModal
        isOpen={!!subjectToDelete}
        subject={subjectToDelete}
        onClose={() => setSubjectToDelete(null)}
      />
    </div>
  )
}
