namespace Neovolve.BuildTaskExecutor.Tasks
{
    /// <summary>
    /// The <see cref="FileMatchResult"/>
    ///   enum defines the result of a file match.
    /// </summary>
    /// <remarks>
    /// The <see cref="WildcardFileSearchTask"/> class uses the
    ///   <see cref="FileMatchResult"/> value to determine whether to continue with file searching.
    /// </remarks>
    public enum FileMatchResult
    {
        /// <summary>
        /// Indicates that the file search should continue.
        /// </summary>
        Continue = 0, 

        /// <summary>
        /// Cancel the file search and mark the task as successful.
        /// </summary>
        Cancel, 

        /// <summary>
        /// Cancel the file search and mark the task as failed.
        /// </summary>
        FailTask
    }
}