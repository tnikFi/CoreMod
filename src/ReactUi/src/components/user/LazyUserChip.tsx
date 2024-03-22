import React from 'react'
import UserChip, { UserChipProps } from './UserChip'
import { UserDto } from '../../api'
import { SelectedGuildContext } from '../../contexts/SelectedGuildContext'
import UserInfoService from '../../services/UserInfoService'

interface LazyUserChipProps extends UserChipProps {
  username?: string
  defaultLabel?: string
}

const LazyUserChip: React.FC<LazyUserChipProps> = ({ userId, defaultLabel }) => {
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
    <UserChip
      userId={userId}
      username={user?.username || defaultLabel || 'Loading...'}
      nickname={user?.nickname}
      avatarUrl={user?.icon}
      color={user?.color || undefined}
      loading={!user}
    />
  )
}

export default LazyUserChip
