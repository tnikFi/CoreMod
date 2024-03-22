import { GuildsService, UserDto } from '../api'
import { isDefaultRoleColor } from '../utils/ColorUtils'

interface UserCache<T> {
  [userId: string]: T
}

/**
 * Service to get user information from the API. Automatically caches the results to prevent duplicate requests.
 * Cache is invalidated when guildId changes.
 */
class UserInfoService {
  private guildId?: string
  private cache: UserCache<UserDto> = {}
  private queue: UserCache<Promise<UserDto>> = {}

  /**
   * Attempts to get user information from the cache or API. Does not handle errors.
   * @param guildId ID of the guild the user is in
   * @param userId ID of the user to get information for
   * @returns Promise that resolves to the user information if successful
   */
  async getUserInfo(guildId: string, userId: string) {
    if (this.guildId !== guildId) {
      this.guildId = guildId
      this.cache = {}
      this.queue = {}
    }

    if (this.cache[userId]) {
      return this.cache[userId]
    }

    if (this.queue[userId] !== undefined) {
      return this.queue[userId]
    }

    this.queue[userId] = GuildsService.getApiGuildsUser(guildId, userId)
      .then((response) => {
        if (response.color && isDefaultRoleColor(response.color)) response.color = undefined
        this.cache[userId] = response
        delete this.queue[userId]
        return response
      })
      .catch((error) => {
        console.error(error)
        delete this.queue[userId]
        throw error
      })

    return this.queue[userId]
  }
}

export default new UserInfoService()
