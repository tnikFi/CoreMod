import React from 'react'
import UserChip, { UserChipProps } from './UserChip'
import { UserDto } from '../../api'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import UserInfoService from '../../services/UserInfoService'

interface LazyUserChipProps extends UserChipProps {
  username?: string
  defaultLabel?: string
}

const LazyUserChip: React.FC<LazyUserChipProps> = ({ userId, defaultLabel, ...props }) => {
  const { selectedGuild } = React.useContext(SelectedGuildContext)
  const [user, setUser] = React.useState<UserDto | null>(null)

  React.useEffect(() => {
    const fetchUser = async () => {
      // Fetch the user by ID
      if (!selectedGuild?.id) return
      try {
        const response = await UserInfoService.getUserInfo(selectedGuild.id, userId)
        setUser(response)
      } catch (error) {
        console.error(error)
      }
    }
    fetchUser()
  }, [selectedGuild, userId])

  return (
    <React.Suspense fallback={<UserChip userId={userId} username={defaultLabel || 'Loading...'} loading />}>
      <UserChip
        userId={userId}
        username={user?.username || defaultLabel || 'Unknown User'}
        nickname={user?.nickname}
        avatarUrl={user?.icon}
        color={user?.color}
        disableUsernameCopy={user?.username === undefined}
        {...props}
      />
    </React.Suspense>
  )
}

export default LazyUserChip
