import { Flame, Trophy, Zap, Target, Star, BookOpen } from 'lucide-react'
import { useTranslation } from 'react-i18next'

function StatCard({
  icon,
  value,
  label,
  color,
}: {
  icon: React.ReactNode
  value: string
  label: string
  color: string
}) {
  return (
    <div className="flex items-center gap-3 rounded-xl border border-primary-foreground/10 bg-primary-foreground/10 px-4 py-3 backdrop-blur-sm">
      <div
        className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg"
        style={{ backgroundColor: color }}
      >
        {icon}
      </div>
      <div>
        <p className="text-lg leading-tight font-bold text-primary-foreground">
          {value}
        </p>
        <p className="text-xs text-primary-foreground/60">{label}</p>
      </div>
    </div>
  )
}

function BadgeItem({ label }: { label: string }) {
  return (
    <span className="inline-flex items-center gap-1.5 rounded-full border border-primary-foreground/10 bg-primary-foreground/10 px-3 py-1.5 text-xs font-medium text-primary-foreground/80 backdrop-blur-sm">
      <Star
        className="h-3 w-3 fill-current text-[hsl(38,92%,55%)]"
        aria-hidden="true"
      />
      {label}
    </span>
  )
}

export function BrandingPanel() {
  const { t } = useTranslation()

  return (
    <div className="relative flex h-full w-full flex-col items-center justify-center overflow-hidden bg-primary px-12 py-16">
      {/* Dot pattern background */}
      <div
        className="pointer-events-none absolute inset-0 opacity-[0.08]"
        style={{
          backgroundImage:
            'radial-gradient(circle, hsl(0 0% 100%) 1px, transparent 1px)',
          backgroundSize: '28px 28px',
        }}
        aria-hidden="true"
      />

      <div className="relative z-10 flex max-w-md flex-col items-center gap-10 text-center">
        {/* Logo */}
        <div className="flex items-center gap-3">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary-foreground/15 backdrop-blur-sm">
            <BookOpen
              className="h-6 w-6 text-primary-foreground"
              aria-hidden="true"
            />
          </div>
          <span className="text-2xl font-bold tracking-tight text-primary-foreground">
            {t('system.name')}
          </span>
        </div>

        {/* Headline */}
        <div className="flex flex-col gap-3">
          <h2 className="text-4xl font-bold tracking-tight text-balance text-primary-foreground">
            {t('brandingPanel.title')}
          </h2>
          <p className="text-base leading-relaxed text-pretty text-primary-foreground/70">
            {t('brandingPanel.description')}
          </p>
        </div>

        {/* Gamification stat cards */}
        <div className="grid w-full grid-cols-2 gap-3">
          <StatCard
            icon={<Flame className="h-5 w-5 text-primary-foreground" />}
            value="12-day"
            label="Avg. streak"
            color="hsl(15, 80%, 55%)"
          />
          <StatCard
            icon={<Zap className="h-5 w-5 text-primary-foreground" />}
            value="4,200 XP"
            label="Earned weekly"
            color="hsl(38, 92%, 50%)"
          />
          <StatCard
            icon={<Trophy className="h-5 w-5 text-primary-foreground" />}
            value="#3"
            label="Leaderboard"
            color="hsl(262, 60%, 52%)"
          />
          <StatCard
            icon={<Target className="h-5 w-5 text-primary-foreground" />}
            value="87%"
            label="Accuracy rate"
            color="hsl(215, 72%, 52%)"
          />
        </div>

        {/* Recent badges */}
        <div className="flex flex-col items-center gap-3">
          <p className="text-xs font-semibold tracking-wider text-primary-foreground/50 uppercase">
            Recent achievements
          </p>
          <div className="flex flex-wrap items-center justify-center gap-2">
            <BadgeItem label="Speed Demon" />
            <BadgeItem label="Perfect Score" />
            <BadgeItem label="7-Day Streak" />
            <BadgeItem label="Quiz Master" />
          </div>
        </div>

        {/* Social proof */}
        <div className="flex items-center gap-4">
          {/* Stacked avatars */}
          <div className="flex -space-x-2">
            {['SM', 'JK', 'AL', 'RP'].map((initials) => (
              <div
                key={initials}
                className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-primary bg-primary-foreground/20 text-[10px] font-bold text-primary-foreground backdrop-blur-sm"
              >
                {initials}
              </div>
            ))}
          </div>
          <p className="text-sm text-primary-foreground/60">
            <span className="font-semibold text-primary-foreground">
              24,000+
            </span>{' '}
            learners this week
          </p>
        </div>
      </div>
    </div>
  )
}
