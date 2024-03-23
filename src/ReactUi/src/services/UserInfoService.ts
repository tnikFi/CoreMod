import { GuildsService, UserDto } from '../api'

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
   * @throws ApiError if the request fails
   */
  async getUserInfo(guildId: string, userId: string) {
    // Invalidate cache and queue if the guild ID changes. (users of a different guild are not relevant to the current guild)
    if (this.guildId !== guildId) {
      this.guildId = guildId
      this.cache = {}
      this.queue = {}
    }

    // Return cached user information if available
    if (this.cache[userId]) {
      return this.cache[userId]
    }

    // Return the pending promise if the information for this user is already being fetched
    if (this.queue[userId] !== undefined) {
      return this.queue[userId]
    }

    // Fetch user information from the API, add the promise to the queue, and cache the result
    this.queue[userId] = GuildsService.getApiGuildsUser(guildId, userId)
      .then((response) => {
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
