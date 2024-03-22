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
  const [loading, setLoading] = React.useState(true)

  React.useEffect(() => {
    const fetchUser = async () => {
      // Fetch the user by ID
      if (!selectedGuild?.id) return
      try {
        const response = await UserInfoService.getUserInfo(selectedGuild.id, userId)
        setUser(response)
      } catch (error) {
        console.error(error)
      } finally {
        setLoading(false)
      }
    }
    fetchUser()
  }, [selectedGuild, userId])

  return (
    <>
      {user ? (
        <UserChip
          userId={userId}
          username={user.username || defaultLabel}
          nickname={user.nickname}
          avatarUrl={user.icon}
          color={user.color || undefined}
        />
      ) : (
        <UserChip
          userId={userId}
          username={loading ? defaultLabel || 'Loading...' : `Unknown User ${userId}`}
          avatarUrl={null}
          loading={loading}
        />
      )}
    </>
  )
}

export default LazyUserChip
