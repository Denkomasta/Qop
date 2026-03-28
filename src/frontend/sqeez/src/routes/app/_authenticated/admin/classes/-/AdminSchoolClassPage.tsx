import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { School, Search } from 'lucide-react'

import { DebouncedInput } from '@/components/ui/Input/DebouncedInput'
import { Pagination } from '@/components/ui/Pagination'
import { useGetApiClasses } from '@/api/generated/endpoints/school-classes/school-classes'
import type { SchoolClassDto } from '@/api/generated/model'

import { AdminSchoolClassTable } from './AdminSchoolClassTable'
import { TeacherModificationModal } from './TeacherModificationModal'

export function AdminSchoolClassPage() {
  const { t } = useTranslation()

  const [searchQuery, setSearchQuery] = useState('')
  const [academicYearFilter, setAcademicYearFilter] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const pageSize = 15

  const [selectedClassForTeacher, setSelectedClassForTeacher] =
    useState<SchoolClassDto | null>(null)

  const { data: classesResponse, isLoading } = useGetApiClasses({
    SearchTerm: searchQuery || undefined,
    AcademicYear: academicYearFilter || undefined,
    PageNumber: pageNumber,
    PageSize: pageSize,
  })

  const classes = classesResponse?.data || []
  const totalPages = Number(classesResponse?.totalPages || 1)
  const totalCount = classesResponse?.totalCount || 0

  return (
    <div className="flex h-full flex-col bg-background">
      <header className="border-b border-border bg-card p-6">
        <div className="mx-auto flex max-w-7xl flex-col gap-6">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10 text-primary">
              <School className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-3xl font-bold tracking-tight">
                {t('admin.classes.classManagement')}
              </h1>
              <p className="text-muted-foreground">
                {t('admin.classes.totalClasses')}:{' '}
                <span className="font-bold">{totalCount}</span>
              </p>
            </div>
          </div>

          <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
            <DebouncedInput
              id="admin-class-search"
              value={searchQuery}
              onChange={(val) => {
                setSearchQuery(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.classes.searchClasses')}
              icon={<Search className="h-4 w-4" />}
              className="max-w-md bg-background"
              hideErrors
            />

            <DebouncedInput
              id="admin-academic-year"
              value={academicYearFilter}
              onChange={(val) => {
                setAcademicYearFilter(val)
                setPageNumber(1)
              }}
              placeholder={t('admin.filterYear', 'e.g. 2024/2025')}
              className="max-w-50 bg-background"
              hideErrors
            />
          </div>
        </div>
      </header>

      <main className="flex-1 overflow-y-auto p-6">
        <div className="mx-auto max-w-7xl">
          <AdminSchoolClassTable
            classes={classes}
            isLoading={isLoading}
            onEditTeacher={setSelectedClassForTeacher}
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
      </main>

      <TeacherModificationModal
        key={selectedClassForTeacher?.id ?? 'empty-modal'}
        isOpen={!!selectedClassForTeacher}
        onClose={() => setSelectedClassForTeacher(null)}
        schoolClass={selectedClassForTeacher}
      />
    </div>
  )
}
