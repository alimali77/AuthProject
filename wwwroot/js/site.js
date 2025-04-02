// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize all tooltips
    initTooltips();

    // Initialize profile picture upload preview
    initProfilePictureUpload();

    // Add smooth scrolling to all page anchors
    initSmoothScroll();

    // Initialize password strength meter if on register page
    initPasswordStrengthMeter();

    // Add animation to alert messages
    initAlertAnimations();

    // Make cards hoverable for better UX
    initHoverableCards();
});

// Initialize Bootstrap tooltips
function initTooltips() {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Preview profile picture before upload
function initProfilePictureUpload() {
    const profilePictureInput = document.getElementById('ProfilePictureFile');
    if (profilePictureInput) {
        profilePictureInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const previewImg = document.querySelector('.profile-picture');
                    if (previewImg) {
                        previewImg.src = e.target.result;
                    }
                }
                reader.readAsDataURL(file);
            }
        });
    }
}

// Smooth scrolling for anchor links
function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Password strength meter
function initPasswordStrengthMeter() {
    // Changed 'Password' to lowercase 'password' to match the ID in the view
    const passwordInput = document.getElementById('password');
    const strengthMeter = document.getElementById('password-strength-meter');
    const strengthText = document.getElementById('password-strength-text');

    if (passwordInput && strengthMeter && strengthText) {
        passwordInput.addEventListener('input', function () {
            const password = passwordInput.value;
            const strength = calculatePasswordStrength(password);

            // Update the strength meter
            strengthMeter.value = strength;

            // Update the text and color
            let strengthLabel = '';
            let strengthColor = '';

            if (strength === 0) {
                strengthLabel = 'No password';
                strengthColor = '#e2e8f0';
            } else if (strength < 25) {
                strengthLabel = 'Very weak';
                strengthColor = '#ef4444';
            } else if (strength < 50) {
                strengthLabel = 'Weak';
                strengthColor = '#f59e0b';
            } else if (strength < 75) {
                strengthLabel = 'Medium';
                strengthColor = '#3b82f6';
            } else {
                strengthLabel = 'Strong';
                strengthColor = '#10b981';
            }

            strengthText.textContent = strengthLabel;
            strengthText.style.color = strengthColor;
            strengthMeter.style.accentColor = strengthColor;
        });
    }
}

// Calculate password strength
function calculatePasswordStrength(password) {
    if (!password) return 0;

    let strength = 0;

    // Length contribution (up to 25 points)
    strength += Math.min(password.length * 2.5, 25);

    // Complexity contribution
    if (/[a-z]/.test(password)) strength += 10; // lowercase
    if (/[A-Z]/.test(password)) strength += 15; // uppercase
    if (/[0-9]/.test(password)) strength += 15; // numbers
    if (/[^A-Za-z0-9]/.test(password)) strength += 20; // special chars

    // Variety contribution
    const uniqueChars = new Set(password.split('')).size;
    strength += Math.min(uniqueChars * 1.5, 15); // up to 15 points for variety

    return Math.min(strength, 100); // Cap at 100
}

// Add animation to alert messages
function initAlertAnimations() {
    document.querySelectorAll('.alert').forEach(alert => {
        // Add animation class
        alert.classList.add('fade-in');

        // Auto dismiss alerts after 5 seconds (except for errors)
        if (!alert.classList.contains('alert-danger')) {
            setTimeout(() => {
                alert.style.opacity = '0';
                alert.style.transition = 'opacity 0.5s ease';
                setTimeout(() => {
                    alert.style.display = 'none';
                }, 500);
            }, 5000);
        }

        // Add close button functionality
        const closeButton = alert.querySelector('.close, .btn-close');
        if (closeButton) {
            closeButton.addEventListener('click', function () {
                alert.style.opacity = '0';
                alert.style.transition = 'opacity 0.5s ease';
                setTimeout(() => {
                    alert.style.display = 'none';
                }, 500);
            });
        }
    });
}

// Make cards hoverable
function initHoverableCards() {
    document.querySelectorAll('.hover-lift').forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-5px)';
            this.style.boxShadow = '0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)';
            this.style.transition = 'transform 0.3s ease, box-shadow 0.3s ease';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '';
        });
    });
}

// Toggle password visibility
function togglePasswordVisibility(inputId, toggleIconId) {
    const passwordInput = document.getElementById(inputId);
    const toggleIcon = document.getElementById(toggleIconId);

    if (passwordInput && toggleIcon) {
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            toggleIcon.classList.remove('bi-eye');
            toggleIcon.classList.add('bi-eye-slash');
        } else {
            passwordInput.type = 'password';
            toggleIcon.classList.remove('bi-eye-slash');
            toggleIcon.classList.add('bi-eye');
        }
    }
}

// For admin page - confirm user deletion
function confirmDelete(formId) {
    if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
        document.getElementById(formId).submit();
    }
    return false;
}

// Show loader while form is submitting
function showLoader(buttonId) {
    const button = document.getElementById(buttonId);
    if (button) {
        const originalText = button.innerHTML;
        button.innerHTML = '<span class="loader"></span> Processing...';
        button.disabled = true;

        // Store original text to restore later
        button.setAttribute('data-original-text', originalText);

        return true; // Allow form submission
    }
    return false;
}

// Restore button after form submission
function restoreButton(buttonId) {
    const button = document.getElementById(buttonId);
    if (button) {
        const originalText = button.getAttribute('data-original-text');
        if (originalText) {
            button.innerHTML = originalText;
            button.disabled = false;
        }
    }
}