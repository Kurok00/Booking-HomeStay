// Hotels Lazy Loading Script
const hotelGrid = document.getElementById('hotelGrid');
const loadingSpinner = document.getElementById('loadingSpinner');
const endMessage = document.getElementById('endMessage');

// Lazy loading state
let currentPage = 1;
let isLoading = false;
let hasMoreData = true;
let currentFilters = {
    search: '',
    sortBy: '',
    checkIn: '',
    checkOut: '',
    city: ''
};

// Load more hotels when scroll to bottom
function handleScroll() {
    if (isLoading || !hasMoreData) return;

    const scrollPosition = window.innerHeight + window.scrollY;
    const threshold = document.documentElement.scrollHeight - 500; // Load 500px before bottom

    if (scrollPosition >= threshold) {
        loadMoreHotels();
    }
}

async function loadMoreHotels() {
    if (isLoading || !hasMoreData) return;

    isLoading = true;
    loadingSpinner.style.display = 'block';
    endMessage.style.display = 'none';

    currentPage++;

    try {
        const params = new URLSearchParams();
        if (currentFilters.search) params.append('search', currentFilters.search);
        if (currentFilters.sortBy) params.append('sortBy', currentFilters.sortBy);
        if (currentFilters.checkIn) params.append('checkIn', currentFilters.checkIn);
        if (currentFilters.checkOut) params.append('checkOut', currentFilters.checkOut);
        if (currentFilters.city) params.append('city', currentFilters.city);
        params.append('page', currentPage);
        params.append('pageSize', 15);

        const response = await fetch(`/Hotels/LoadMoreHotels?${params.toString()}`);
        const html = await response.text();

        if (html.trim().length === 0) {
            hasMoreData = false;
            endMessage.style.display = 'block';
        } else {
            hotelGrid.insertAdjacentHTML('beforeend', html);
        }
    } catch (error) {
        console.error('Error loading more hotels:', error);
    } finally {
        isLoading = false;
        loadingSpinner.style.display = 'none';
    }
}

// Reset pagination when filters change
function resetPagination() {
    currentPage = 1;
    hasMoreData = true;
    endMessage.style.display = 'none';
}

// Initialize lazy loading with debounce
let scrollTimeout;
window.addEventListener('scroll', () => {
    clearTimeout(scrollTimeout);
    scrollTimeout = setTimeout(handleScroll, 100); // Debounce 100ms
});
