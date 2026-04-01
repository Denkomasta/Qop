import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { FileUp, Info, Table as TableIcon } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/Card'
import { ImportCsvModal } from './ImportCsvModal'

export function AdminImportPage() {
  const { t } = useTranslation()
  const [isModalOpen, setIsModalOpen] = useState(false)

  const csvTemplateHeaders = [
    'Class Name',
    'Academic Year',
    'Subject Name',
    'Subject Code',
    'First Name',
    'Last Name',
    'Email',
    'Password',
  ]

  return (
    <div className="flex h-full flex-col bg-background">
      <div className="border-b border-border bg-card p-6">
        <div className="mx-auto flex max-w-5xl flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight text-foreground">
              {t('admin.import.title')}
            </h1>
            <p className="mt-1 text-sm text-muted-foreground">
              {t('admin.import.subtitle')}
            </p>
          </div>
          <Button onClick={() => setIsModalOpen(true)} className="gap-2">
            <FileUp className="h-4 w-4" />
            {t('admin.import.startBtn')}
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-6">
        <div className="mx-auto max-w-5xl space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Info className="h-5 w-5 text-blue-500" />
                {t('admin.import.howItWorks')}
              </CardTitle>
              <CardDescription>{t('admin.import.description')}</CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4 md:grid-cols-3">
              <div className="rounded-lg border p-4">
                <h3 className="mb-2 flex items-center gap-2 font-bold">
                  <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">
                    1
                  </span>
                  {t('admin.import.steps.classes.title')}
                </h3>
                <p className="text-xs text-muted-foreground">
                  {t('admin.import.steps.classes.desc')}
                </p>
              </div>

              <div className="rounded-lg border p-4">
                <h3 className="mb-2 flex items-center gap-2 font-bold">
                  <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">
                    2
                  </span>
                  {t('admin.import.steps.subjects.title')}
                </h3>
                <p className="text-xs text-muted-foreground">
                  {t('admin.import.steps.subjects.desc')}
                </p>
              </div>

              <div className="rounded-lg border p-4">
                <h3 className="mb-2 flex items-center gap-2 font-bold">
                  <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">
                    3
                  </span>
                  {t('admin.import.steps.students.title')}
                </h3>
                <p className="text-xs text-muted-foreground">
                  {t('admin.import.steps.students.desc')}
                </p>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <TableIcon className="h-5 w-5 text-primary" />
                {t('admin.import.csvFormat')}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto rounded-md border">
                <table className="w-full text-left text-sm">
                  <thead className="bg-muted">
                    <tr>
                      {csvTemplateHeaders.map((header) => (
                        <th key={header} className="border-b p-3 font-semibold">
                          {header}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    <tr className="text-muted-foreground">
                      <td className="border-b p-3 italic">
                        {t('admin.import.tableExample.className')}
                      </td>
                      <td className="border-b p-3 italic">2025/2026</td>
                      <td className="border-b p-3 italic">
                        {t('admin.import.tableExample.subjectName')}
                      </td>
                      <td className="border-b p-3 italic">MATH-01</td>
                      <td className="border-b p-3 italic">
                        {t('admin.import.tableExample.firstName')}
                      </td>
                      <td className="border-b p-3 italic">
                        {t('admin.import.tableExample.lastName')}
                      </td>
                      <td className="border-b p-3 italic">
                        {t('admin.import.tableExample.email')}
                      </td>
                      <td className="border-b p-3 italic">********</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <p className="mt-4 text-xs text-muted-foreground">
                *{' '}
                {t(
                  'admin.import.passwordNote',
                  'If password is left empty, a default one will be assigned.',
                )}
              </p>
            </CardContent>
          </Card>
        </div>
      </div>

      <ImportCsvModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
      />
    </div>
  )
}
